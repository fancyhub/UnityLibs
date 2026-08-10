'use strict';

const fs = require('fs');

const EOCD_SIGNATURE = 0x06054b50;
const APK_SIG_BLOCK_MAGIC_LO = 0x20676953204b5041n; // "APK Sig "
const APK_SIG_BLOCK_MAGIC_HI = 0x3234206b636f6c42n; // "Block 42"
const UINT32_MAX = 0xffffffff;
const ZIP_COMMENT_MAX_LENGTH = 0xffff;

function readZipCommentData(apkPath) {
    const apk = fs.readFileSync(apkPath);
    const eocdOffset = findEocdOffset(apk);
    const commentLength = apk.readUInt16LE(eocdOffset + 20);
    return Buffer.from(apk.subarray(eocdOffset + 22, eocdOffset + 22 + commentLength));
}

function writeZipCommentData(inputApkPath, outputApkPath, data) {
    assertDifferentPath(inputApkPath, outputApkPath);
    assertBuffer(data, 'data');

    if (data.length > ZIP_COMMENT_MAX_LENGTH) {
        throw new Error(`ZIP comment data is too large: ${data.length} > ${ZIP_COMMENT_MAX_LENGTH}`);
    }

    const apk = fs.readFileSync(inputApkPath);
    const eocdOffset = findEocdOffset(apk);
    const eocd = Buffer.from(apk.subarray(eocdOffset, eocdOffset + 22));
    eocd.writeUInt16LE(data.length, 20);

    const output = Buffer.concat([
        apk.subarray(0, eocdOffset),
        eocd,
        data,
    ]);

    fs.writeFileSync(outputApkPath, output);
}

function readSigningBlockData(apkPath) {
    const apk = fs.readFileSync(apkPath);
    const signingBlock = parseSigningBlock(apk);
    return signingBlock.entries.map(entry => ({
        entryId: entry.entryId,
        data: Buffer.from(entry.data),
    }));
}

function readSigningBlockEntryData(apkPath, entryId) {
    validateEntryId(entryId);

    const entries = readSigningBlockData(apkPath);
    const entry = entries.find(item => item.entryId === entryId);
    return entry ? entry.data : null;
}

function writeSigningBlockData(inputApkPath, outputApkPath, entries) {
    assertDifferentPath(inputApkPath, outputApkPath);

    if (!Array.isArray(entries)) {
        throw new TypeError('entries must be an array');
    }

    const pairBuffers = entries.map((entry, index) => {
        if (!entry || typeof entry !== 'object') {
            throw new TypeError(`entries[${index}] must be an object`);
        }

        validateEntryId(entry.entryId);
        assertBuffer(entry.data, `entries[${index}].data`);
        return createSigningBlockPair(entry.entryId, entry.data);
    });

    const apk = fs.readFileSync(inputApkPath);
    const signingBlock = parseSigningBlock(apk);
    writeSigningBlockPairs(apk, signingBlock, outputApkPath, pairBuffers);
}

function writeSigningBlockEntryData(inputApkPath, outputApkPath, entryId, data) {
    assertDifferentPath(inputApkPath, outputApkPath);
    validateEntryId(entryId);
    assertBuffer(data, 'data');

    const apk = fs.readFileSync(inputApkPath);
    const signingBlock = parseSigningBlock(apk);
    const pairBuffers = signingBlock.entries
        .filter(entry => entry.entryId !== entryId)
        .map(entry => entry.raw);

    pairBuffers.push(createSigningBlockPair(entryId, data));
    writeSigningBlockPairs(apk, signingBlock, outputApkPath, pairBuffers);
}

function parseSigningBlock(apk) {
    const eocdOffset = findEocdOffset(apk);
    const centralDirectoryOffset = apk.readUInt32LE(eocdOffset + 16);

    if (centralDirectoryOffset === UINT32_MAX) {
        throw new Error('ZIP64 APK is not supported');
    }

    if (centralDirectoryOffset < 32) {
        throw new Error('APK Signing Block is missing');
    }

    const footerOffset = centralDirectoryOffset - 24;
    const sizeInFooter = readUInt64LEAsNumber(apk, footerOffset);
    const magicLo = apk.readBigUInt64LE(footerOffset + 8);
    const magicHi = apk.readBigUInt64LE(footerOffset + 16);

    if (magicLo !== APK_SIG_BLOCK_MAGIC_LO || magicHi !== APK_SIG_BLOCK_MAGIC_HI) {
        throw new Error('APK Signing Block magic not found');
    }

    const blockOffset = centralDirectoryOffset - sizeInFooter - 8;
    if (blockOffset < 0) {
        throw new Error('Invalid APK Signing Block size');
    }

    const sizeInHeader = readUInt64LEAsNumber(apk, blockOffset);
    if (sizeInHeader !== sizeInFooter) {
        throw new Error('APK Signing Block header/footer size mismatch');
    }

    const entries = [];
    let cursor = blockOffset + 8;
    const entriesEnd = footerOffset;

    while (cursor < entriesEnd) {
        const pairOffset = cursor;
        const pairLength = readUInt64LEAsNumber(apk, cursor);
        cursor += 8;

        if (pairLength < 4 || cursor + pairLength > entriesEnd) {
            throw new Error('Invalid APK Signing Block pair length');
        }

        const entryId = apk.readUInt32LE(cursor);
        const dataStart = cursor + 4;
        const dataEnd = cursor + pairLength;

        entries.push({
            entryId,
            data: apk.subarray(dataStart, dataEnd),
            raw: Buffer.from(apk.subarray(pairOffset, dataEnd)),
        });

        cursor = dataEnd;
    }

    if (cursor !== entriesEnd) {
        throw new Error('Invalid APK Signing Block entries');
    }

    return {
        eocdOffset,
        centralDirectoryOffset,
        blockOffset,
        blockEndOffset: centralDirectoryOffset,
        entries,
    };
}

function writeSigningBlockPairs(apk, signingBlock, outputApkPath, pairBuffers) {
    const newSigningBlock = createSigningBlock(pairBuffers);
    const oldSigningBlockLength = signingBlock.blockEndOffset - signingBlock.blockOffset;
    const delta = newSigningBlock.length - oldSigningBlockLength;
    const newCentralDirectoryOffset = signingBlock.centralDirectoryOffset + delta;

    if (newCentralDirectoryOffset > UINT32_MAX) {
        throw new Error('Central directory offset exceeds uint32 range');
    }

    const output = Buffer.concat([
        apk.subarray(0, signingBlock.blockOffset),
        newSigningBlock,
        apk.subarray(signingBlock.blockEndOffset),
    ]);

    output.writeUInt32LE(newCentralDirectoryOffset, signingBlock.eocdOffset + delta + 16);
    fs.writeFileSync(outputApkPath, output);
}

function createSigningBlock(pairBuffers) {
    const pairsLength = pairBuffers.reduce((total, pair) => total + pair.length, 0);
    const blockSize = pairsLength + 24;

    const header = Buffer.alloc(8);
    header.writeBigUInt64LE(BigInt(blockSize), 0);

    const footer = Buffer.alloc(24);
    footer.writeBigUInt64LE(BigInt(blockSize), 0);
    footer.writeBigUInt64LE(APK_SIG_BLOCK_MAGIC_LO, 8);
    footer.writeBigUInt64LE(APK_SIG_BLOCK_MAGIC_HI, 16);

    return Buffer.concat([header, ...pairBuffers, footer]);
}

function createSigningBlockPair(entryId, data) {
    const pairLength = 4 + data.length;
    const pair = Buffer.alloc(8 + pairLength);
    pair.writeBigUInt64LE(BigInt(pairLength), 0);
    pair.writeUInt32LE(entryId, 8);
    data.copy(pair, 12);
    return pair;
}

function findEocdOffset(apk) {
    const minEocdLength = 22;
    const maxCommentLength = ZIP_COMMENT_MAX_LENGTH;
    const searchStart = Math.max(0, apk.length - minEocdLength - maxCommentLength);

    for (let offset = apk.length - minEocdLength; offset >= searchStart; offset--) {
        if (apk.readUInt32LE(offset) !== EOCD_SIGNATURE) {
            continue;
        }

        const commentLength = apk.readUInt16LE(offset + 20);
        if (offset + minEocdLength + commentLength === apk.length) {
            return offset;
        }
    }

    throw new Error('ZIP End of Central Directory record not found');
}

function readUInt64LEAsNumber(buffer, offset) {
    const value = buffer.readBigUInt64LE(offset);
    if (value > BigInt(Number.MAX_SAFE_INTEGER)) {
        throw new Error('uint64 value exceeds Number.MAX_SAFE_INTEGER');
    }

    return Number(value);
}

function validateEntryId(entryId) {
    if (!Number.isInteger(entryId) || entryId < 0 || entryId > UINT32_MAX) {
        throw new TypeError('entryId must be a uint32 number');
    }
}

function assertBuffer(value, name) {
    if (!Buffer.isBuffer(value)) {
        throw new TypeError(`${name} must be a Buffer`);
    }
}

function assertDifferentPath(inputApkPath, outputApkPath) {
    if (inputApkPath === outputApkPath) {
        throw new Error('inputApkPath and outputApkPath must be different paths');
    }
}

module.exports = {
    readZipCommentData,
    writeZipCommentData,
    readSigningBlockData,
    readSigningBlockEntryData,
    writeSigningBlockData,
    writeSigningBlockEntryData,
};

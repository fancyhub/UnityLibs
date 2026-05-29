'use strict';
const { execFileSync } = require('child_process');
const fs = require('fs');
const path = require('path');

// Set to false to hide keystore passwords in printed commands.
const PRINT_COMMAND_PASSWORDS = true;

/**
 * Signing and Android tool paths loaded from config.json.
 */
class Config {
    keyStoreFilePath = '';
    keyStorePassword = '';
    keyAlias = '';
    keyPassword = '';

    apksignerPath = '';
    zipalignPath = '';
    bundletoolPath = '';
}


/**
 * Masks password argument values before printing a command.
 * @param {string[]} args command arguments
 * @returns {string[]} copied arguments with password values replaced
 */
function maskCommandPasswordArgs(args) {
    const maskedArgs = [];
    const passwordOptions = new Set(['-storepass', '-keypass', '--ks-pass', '--key-pass']);

    for (let i = 0; i < args.length; i++) {
        const arg = args[i];
        if (passwordOptions.has(arg)) {
            maskedArgs.push(arg);
            if (i + 1 < args.length) {
                maskedArgs.push('******');
                i++;
            }
            continue;
        }

        if (arg.startsWith('--ks-pass=') || arg.startsWith('--key-pass=')) {
            maskedArgs.push(arg.replace(/=.*/, '=******'));
            continue;
        }

        maskedArgs.push(arg);
    }

    return maskedArgs;
}

/**
 * Runs an external executable with arguments.
 * Exits the process with code 1 if the executable fails.
 * @param {string} cmdName executable name or absolute path, such as adb, jar, or zipalign
 * @param {string[]} args executable arguments
 * @param {string} name display name used in logs
 * @param {object} [options] extra execFileSync options
 * @returns {boolean} true when the executable succeeds
 */
function execCommand(cmdName, args, name, options = {}) {

    //print command
    {
        const displayArgs = PRINT_COMMAND_PASSWORDS ? args : maskCommandPasswordArgs(args);
        let displayCommand = [cmdName, ...displayArgs].map(arg => {
            if (arg === '') {
                return '""';
            }
            return /[\s"&|<>^]/.test(arg) ? `"${arg.replace(/"/g, '\\"')}"` : arg;
        }).join(' ');

        console.log(`Exe ${name} Command: \n\t${displayCommand}\n`);
    }

    try {
        execFileSync(cmdName, args, { stdio: 'inherit', ...options });
        return true;
    } catch (error) {
        const message = PRINT_COMMAND_PASSWORDS
            ? error.message
            : `exit code ${error.status ?? 1}`;
        console.error(`Exe ${name} Command Failed: ${message}`);
        process.exit(1);
    }
}

/**
 * Runs an external executable from a specific working directory.
 * @param {string} cmdName executable name or absolute path
 * @param {string[]} args executable arguments
 * @param {string} name display name used in logs
 * @param {string} dir working directory
 * @returns {boolean} true when the executable succeeds
 */
function execCommandInDir(cmdName, args, name, dir) {
    return execCommand(cmdName, args, name, { cwd: dir });
}


/**
 * Signs an AAB or JAR-compatible archive with jarsigner.
 * See https://docs.oracle.com/javase/8/docs/technotes/tools/windows/jarsigner.html
 * @param {Config} config
 * @param {string} filePath archive path to sign
 * @returns {boolean} true when signing succeeds
 */
function signWithJarsigner(config, filePath) {
    const args = [
        '-keystore', config.keyStoreFilePath,
        '-storepass', config.keyStorePassword,
        '-keypass', config.keyPassword,
        '-sigalg', 'SHA256withRSA',
        '-digestalg', 'SHA-256',
        filePath,
        config.keyAlias,
    ];
    return execCommand('jarsigner', args, 'Sign');
}


/**
 * Signs an APK with apksigner.
 * See https://developer.android.com/tools/apksigner
 * @param {Config} config
 * @param {string} filePath APK path to sign
 * @returns {boolean} true when signing succeeds
 */
function signWithApksigner(config, filePath) {
    const args = [
        '-jar', config.apksignerPath,
        'sign',
        '--ks', config.keyStoreFilePath,
        '--ks-pass', `pass:${config.keyStorePassword}`,
        '--ks-key-alias', config.keyAlias,
        '--key-pass', `pass:${config.keyPassword}`,
        '--v1-signing-enabled', 'true',
        '--v2-signing-enabled', 'true',
        '--v3-signing-enabled', 'true',
        '--v4-signing-enabled', 'true',
        filePath,
    ];

    return execCommand('java', args, 'Sign');
}

/**
 * Aligns an APK with zipalign.
 * See https://developer.android.com/tools/zipalign
 * @param {Config} config
 * @param {string} inputFilePath APK path before alignment
 * @param {string} outputFilePath aligned APK output path
 * @returns {boolean} true when alignment succeeds
 */
function zipAlign(config, inputFilePath, outputFilePath) {
    ensureParentDir(outputFilePath);
    const args = [
        '-P', '16',
        '-f',
        // `-v`,
        '4',
        inputFilePath,
        outputFilePath,
    ];

    return execCommand(config.zipalignPath, args, 'ZipAlign');
}

/**
 * Installs an APK on the connected Android device.
 * @param {string} inputFilePath APK path to install
 * @returns {boolean} true when installation succeeds
 */
function installApk(inputFilePath) {
    const args = ['install', '-r', inputFilePath];

    return execCommand('adb', args, 'installApk');
}


/**
 * Installs an APKS bundle on the connected Android device with bundletool.
 * @param {Config} config
 * @param {string} inputFilePath APKS path to install
 * @returns {boolean} true when installation succeeds
 */
function installApks(config, inputFilePath) {
    const args = [
        '-jar', config.bundletoolPath,
        'install-apks',
        `--apks=${inputFilePath}`,
    ];
    return execCommand('java', args, 'installApks');
}


/**
 * Updates an APK/AAB archive with files from a resource directory.
 * @param {string} apkFilePath archive path to update
 * @param {string} resourcesRootDir directory whose contents are inserted into the archive
 * @returns {boolean} true when resource replacement succeeds
 */
function replaceResources(apkFilePath, resourcesRootDir) {
    const args = [
        '-uf',
        apkFilePath,
        '-C', resourcesRootDir,
        './',
    ];

    return execCommandInDir('jar', args, 'replaceResources', resourcesRootDir);
}

/**
 * Prints APK signing certificate details with apksigner.
 * See https://developer.android.com/tools/apksigner
 * @param {Config} config
 * @param {string} inputFilePath APK path to inspect
 * @returns {boolean} true when certificate inspection succeeds
 */
function printCertWithApksigner(config, inputFilePath) {
    const args = [
        '-jar',
        config.apksignerPath,
        'verify',
        '--verbose',
        '--print-certs',
        inputFilePath,
    ];
    return execCommand('java', args, 'Print Cert Of Apk');
}

/**
 * Finds the first APK in a directory, preferring root-level universal.apk.
 * @param {string} dir directory path
 * @returns {string|null} APK file path, or null when no APK exists
 */
function findFirstApkFile(dir) {
    const entries = fs.readdirSync(dir, { withFileTypes: true });

    for (const entry of entries) {
        const filePath = path.join(dir, entry.name);
        if (entry.isFile() && entry.name.toLowerCase() === 'universal.apk') {
            return filePath;
        }
    }

    for (const entry of entries) {
        const filePath = path.join(dir, entry.name);
        if (entry.isDirectory()) {
            const found = findFirstApkFile(filePath);
            if (found != null) {
                return found;
            }
        } else if (path.extname(entry.name).toLowerCase() === '.apk') {
            return filePath;
        }
    }

    return null;
}


/**
 * Prints certificate details for the first APK inside an APKS archive.
 * See https://developer.android.com/tools/apksigner
 * @param {Config} config
 * @param {string} inputFilePath APKS path to inspect
 */
function printCertOfApksWithApkSigner(config, inputFilePath) {
    let tempDir = unzipTarget2Dir(inputFilePath, path.resolve("output/temp"));
    if (tempDir == null) {
        process.exit(1);
    }

    let apkFilePath = findFirstApkFile(tempDir);
    if (apkFilePath == null) {
        console.error("No apk found in apks: " + inputFilePath);
        process.exit(1);
    }
    printCertWithApksigner(config, apkFilePath);
}

/**
 * Converts an AAB to an APKS file with bundletool.
 * See https://developer.android.com/tools/bundletool
 * @param {Config} config
 * @param {string} aabFilePath source AAB path
 * @param {string} apksFilePath output APKS path
 * @param {boolean} universal true to build a universal APK inside the APKS
 * @returns {boolean} true when conversion succeeds
 */
function aab2Apks(config, aabFilePath, apksFilePath, universal) {
    ensureParentDir(apksFilePath);
    const args = [
        '-jar', config.bundletoolPath,
        'build-apks',
        `--bundle=${aabFilePath}`,
        `--output=${apksFilePath}`,
        `--ks=${config.keyStoreFilePath}`,
        `--ks-pass=pass:${config.keyStorePassword}`,
        `--ks-key-alias=${config.keyAlias}`,
        `--key-pass=pass:${config.keyPassword}`,
        '--overwrite',
    ];
    if (universal) {
        args.push('--mode=universal');
    }

    return execCommand('java', args, 'aab2Apks');
}


/**
 * Prints certificate details for an AAB, APKS, or JAR-like archive with jarsigner/keytool.
 * @param {string} inputFilePath archive path to inspect
 * @returns {boolean} true when certificate inspection succeeds
 */
function printJarCert(inputFilePath) {
    const args = [
        '-verify',
        '-verbose:signing',
        '-certs',
        inputFilePath,
    ];

    if (!execCommand('jarsigner', args, 'print cert with jarsigner')) {
        return false;
    }


    if (!unzipTargetMetaInf2Dir(inputFilePath, path.resolve("output/temp"))) {
        return false;
    }

    let metaInfDir = path.resolve("output/temp/META-INF/");
    if (!fs.existsSync(metaInfDir)) {
        return false;
    }

    const fileNames = fs.readdirSync(metaInfDir);
    for (const fileName of fileNames) {
        let extName = path.extname(fileName).toUpperCase();
        if (extName === ".RSA" || extName === ".DSA") {
            const certFilePath = path.resolve(metaInfDir, fileName);
            printCertWithKeytool(certFilePath);
        }
    }
    return true;
}

/**
 * Prints keystore details with keytool.
 * @param {string} keyStoreFilePath keystore file path
 * @param {string} storePassword keystore password
 * @returns {boolean} true when keytool succeeds
 */
function printKeyStoreWithKeytool(keyStoreFilePath, storePassword) {
    const args = [
        '-list',
        '-v',
        '-keystore', keyStoreFilePath,
        '-storepass', storePassword,
    ];
    return execCommand('keytool', args, 'print keystore with keytool');
}


/**
 * Prints certificate file details with keytool.
 * @param {string} certFilePath certificate file path
 * @returns {boolean} true when keytool succeeds
 */
function printCertWithKeytool(certFilePath) {
    const args = [
        '-printcert',
        '-file', certFilePath,
    ];
    return execCommand('keytool', args, 'print cert with keytool');
}


/**
 * Loads the signing/tool configuration JSON.
 * @param {string} configFilePath config.json path
 * @returns {Config|null} parsed config, or null when loading fails
 */
function loadConfig(configFilePath) {
    try {
        const config = JSON.parse(fs.readFileSync(configFilePath, 'utf8'));
        return config;
    } catch (error) {
        console.error('load config file failed, ' + configFilePath);
        return null;
    }
}

/**
 * Resolves and validates required paths from config.
 * @param {Config|null} config config object to validate
 * @returns {boolean} true when all required paths exist
 */
function validateConfig(config) {
    if (config == null)
        return false

    config.keyStoreFilePath = path.resolve(config.keyStoreFilePath)
    if (!fs.existsSync(config.keyStoreFilePath)) {
        console.error("keyStoreFilePath not exist: " + config.keyStoreFilePath)
        return false
    }

    config.apksignerPath = path.resolve(config.apksignerPath)
    if (!fs.existsSync(config.apksignerPath)) {
        console.error("apksignerPath not exist: " + config.apksignerPath)
        return false
    }

    config.zipalignPath = path.resolve(config.zipalignPath)
    if (!fs.existsSync(config.zipalignPath)) {
        console.error("zipalignPath not exist: " + config.zipalignPath)
        return false
    }

    config.bundletoolPath = path.resolve(config.bundletoolPath)
    if (!fs.existsSync(config.bundletoolPath)) {
        console.error("bundletoolPath not exist: " + config.bundletoolPath)
        return false
    }
    return true;
}

/**
 * Creates a sibling file path by appending a suffix before the extension.
 * @param {string} inputFilePath original file path
 * @param {string} appendSuffix suffix to append before the extension
 * @returns {string} new file path
 */
function createNewFilePath(inputFilePath, appendSuffix) {
    let extName = path.extname(inputFilePath);
    return inputFilePath.substring(0, inputFilePath.length - extName.length) + appendSuffix + extName;
}

/**
 * Replaces a file path extension.
 * @param {string} inputFilePath original file path
 * @param {string} newExtName new extension, including the leading dot
 * @returns {string} file path with the new extension
 */
function replaceExtName(inputFilePath, newExtName) {
    let extName = path.extname(inputFilePath);
    return inputFilePath.substring(0, inputFilePath.length - extName.length) + newExtName;
}


/**
 * Creates an empty directory, deleting any existing contents first.
 * @param {string} dir directory path to clear
 */
function clearDir(dir) {
    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true });
        return;
    }

    const entries = fs.readdirSync(dir, { withFileTypes: true });
    for (const entry of entries) {
        if (entry.isDirectory()) {
            fs.rmSync(path.join(dir, entry.name), { recursive: true });
        } else {
            fs.unlinkSync(path.join(dir, entry.name));
        }
    }
}

/**
 * Creates a directory if it does not exist.
 * @param {string} dir directory path
 */
function ensureDir(dir) {
    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true });
    }
}

/**
 * Ensures the parent directory for a file path exists.
 * @param {string} filePath target file path
 */
function ensureParentDir(filePath) {
    ensureDir(path.dirname(filePath));
}

/**
 * Copies a file and optionally updates the target timestamp.
 * @param {string} sourceFilePath source file path
 * @param {string} targetFilePath target file path
 * @param {boolean} changeTime true to set the target timestamp to now
 */
function copyFile(sourceFilePath, targetFilePath, changeTime = false) {
    ensureParentDir(targetFilePath);
    console.log("\tCopy File: " + sourceFilePath + " -> " + targetFilePath);
    fs.copyFileSync(sourceFilePath, targetFilePath);

    if (changeTime) {
        let time = new Date();
        fs.utimesSync(targetFilePath, time, time);
        // fs.utimesSync(targetFilePath, fs.statSync(sourceFilePath).atime, fs.statSync(sourceFilePath).mtime);
    }
}

/**
 * Extracts META-INF from an archive into a directory.
 * @param {string} inputFilePath archive path
 * @param {string} baseDir output directory
 * @returns {boolean} true when extraction succeeds
 */
function unzipTargetMetaInf2Dir(inputFilePath, baseDir) {
    baseDir = path.resolve(baseDir);

    try {
        clearDir(baseDir);

        return execCommandInDir('jar', ['-xf', inputFilePath, 'META-INF/'], 'unzipTargetMetaInf2Dir', baseDir);
    } catch (error) {
        console.error(`unzipTargetMetaInf2Dir Failed: ${error.message}`);
        return false;
    }
}

/**
 * Extracts an entire archive into a directory.
 * @param {string} inputFilePath archive path
 * @param {string} baseDir output directory
 * @returns {string|null} resolved output directory when extraction succeeds, otherwise null
 */
function unzipTarget2Dir(inputFilePath, baseDir) {
    try {
        baseDir = path.resolve(baseDir);
        clearDir(baseDir);

        if (!execCommandInDir('jar', ['-xf', inputFilePath, './'], 'unzip2TempDir', baseDir)) {
            return null;
        }
        return baseDir;
    } catch (error) {
        console.error(`unzip2TempDir Failed: ${error.message}`);
        return null;
    }
}

/**
 * Creates an archive from a directory.
 * @param {string} inputDir directory to archive
 * @param {string} outputFilePath output archive path
 * @returns {boolean} true when archive creation succeeds
 */
function zipDir2Target(inputDir, outputFilePath) {
    ensureParentDir(outputFilePath);
    return execCommandInDir('jar', ['-cf', outputFilePath, '.'], 'zipDir2Target', inputDir);
}


/**
 * Removes existing signature files from META-INF inside an extracted archive.
 * @param {string} baseDir extracted archive root directory
 * @returns {boolean} true when signature cleanup succeeds
 */
function removeSignatureFiles(baseDir) {
    const metaInfDir = path.join(baseDir, 'META-INF');

    try {
        if (!fs.existsSync(metaInfDir)) {
            return true;
        }
        const files = fs.readdirSync(metaInfDir);

        const signatureExtensions = new Set(['.sf', '.dsa', '.rsa', '.ec']);

        for (const file of files) {
            const filePath = path.join(metaInfDir, file);
            const fileExt = path.extname(file).toLowerCase();

            if (signatureExtensions.has(fileExt)) {
                const stat = fs.statSync(filePath);
                if (stat.isFile()) {
                    fs.unlinkSync(filePath);
                }
            }
        }
        return true
    } catch (error) {
        console.error('删除签名文件时出错:', error.message);
        return false
    }
}


/**
 * Reads a single value from stdin.
 * @param {string} name prompt label
 * @returns {string} entered value
 */
function readLineFromStdin(name) {
    process.stdout.write(`${name}:`);
    const buffer = Buffer.alloc(2048);
    const bytesRead = fs.readSync(process.stdin.fd, buffer, 0, buffer.length);
    const input = buffer.toString('utf8', 0, bytesRead).trim();
    return input;
}

/**
 * Gets a command-line argument, prompting for it when missing or empty.
 * @param {number} index process.argv index
 * @param {string} name prompt label
 * @returns {string} argument value
 */
function getArg(index, name) {
    if (process.argv.length <= index) {
        return readLineFromStdin(name);
    }
    if (process.argv[index] === "") {
        return readLineFromStdin(name);
    }
    return process.argv[index];
}



/**
 * Prints command usage.
 */
function printUsage() {
    let selfName = path.basename(__filename)

    console.log(`
Usage: node ${selfName} -config </path/xxx/config.json> cmd
    cmd:
        - resign: resign the apk/aab/apks
            example: node ${selfName} -config </path/xxx/config.json> resign inputFilePath

        - replaceRes: replace the resources about the apk/aab/apks,
            example: node ${selfName} -config </path/xxx/config.json> replaceRes inputFilePath resourceRootDir

        - printCertSelf: print the keystore self
            example: node ${selfName} -config </path/xxx/config.json> printCertSelf

        - printCert: print the cert info about the apk/aab/apks,
            example: node ${selfName} -config </path/xxx/config.json> printCert inputFilePath

        - aab2Apk: convert the aab to apk
            example: node ${selfName} -config </path/xxx/config.json> aab2Apk inputFilePath

        - aab2Apks: convert the aab to apks
            example: node ${selfName} -config </path/xxx/config.json> aab2Apks inputFilePath

        - install: install the apk/aab/apks to the device
            example: node ${selfName} -config </path/xxx/config.json> install inputFilePath
        `);
}

/**
 * Handles the resign command for APK or AAB files.
 * @param {Config} config validated tool configuration
 */
function cmdResign(config) {
    let input = getArg(5, "inputFilePath(apk/aab)");
    if (input === "") {
        printUsage();
        process.exit(1);
    }

    let inputFilePath = path.resolve(input);
    let extName = path.extname(inputFilePath).toLowerCase();
    if (extName === ".apk") {
        //step1: copy apk to temp apk
        console.log("\nstep1/3: copy apk to temp apk================================");
        let outputFilePath = createNewFilePath(inputFilePath, "_new");
        copyFile(inputFilePath, outputFilePath);

        //step2: sign with apksigner
        console.log("\nstep2/3: sign with apksigner================================");
        if (!signWithApksigner(config, outputFilePath))
            process.exit(1);

        //step3: print cert
        console.log("\nstep3/3: print cert================================");
        printCertWithApksigner(config, outputFilePath);

        console.log("\n================================");
        console.log("Success: " + outputFilePath);
        console.log("================================");
    } else if (extName === ".aab") {
        //1. unzip to temp dir
        console.log("\nstep1/5: unzip to temp dir================================");
        let tempDir = path.resolve("output/temp");
        if (unzipTarget2Dir(inputFilePath, tempDir) == null)
            process.exit(1);

        //2. remove signature files
        console.log("\nstep2/5: remove signature files================================");
        if (!removeSignatureFiles(tempDir))
            process.exit(1);

        //3. zip to new aab
        console.log("\nstep3/5: zip to new aab================================");
        let outputFilePath = createNewFilePath(inputFilePath, "_new");
        if (!zipDir2Target(tempDir, outputFilePath))
            process.exit(1);

        //4. sign with jarsigner
        console.log("\nstep4/5: sign with jarsigner================================");
        if (!signWithJarsigner(config, outputFilePath))
            process.exit(1);

        //5. print cert
        console.log("\nstep5/5: print cert================================");
        printJarCert(outputFilePath);

        console.log("\n================================");
        console.log("Success: " + outputFilePath);
        console.log("================================");
    }
    else {
        console.error(`input apk/aab,does't support ${input}`);
        process.exit(1);
    }
}




/**
 * Handles the printCert command for APK, APKS, AAB, or keystore files.
 * @param {Config} config validated tool configuration
 */
function cmdPrintCert(config) {
    let input = getArg(5, "inputFilePath(apk/apks/aab/keystore)")
    if (input === "") {
        printUsage();
        process.exit(1);
    }
    let inputFilePath = path.resolve(input);
    let extName = path.extname(inputFilePath).toLowerCase();
    switch (extName) {
        default:
            console.error(`input APK/AAB/APKS/keystore, does't support ${input}`);
            process.exit(1);
            break;

        case ".apk":
            printCertWithApksigner(config, inputFilePath);
            break;

        case ".apks":
            printCertOfApksWithApkSigner(config, inputFilePath);
            break;

        case ".aab":
            printJarCert(inputFilePath);
            break;

        case ".keystore":
            {
                let password = getArg(6, "password")
                printKeyStoreWithKeytool(inputFilePath, password);
            }
            break

    }
}

/**
 * Handles the replaceRes command for APK or AAB files.
 * @param {Config} config validated tool configuration
 */
function cmdReplaceResource(config) {
    let input = getArg(5, "inputFilePath(apk/apks/aab)")
    if (input === "") {
        printUsage();
        process.exit(1);
    }
    let inputFilePath = path.resolve(input);

    let resourcesRootDir = getArg(6, "resourcesRootDir")
    if (resourcesRootDir === "") {
        printUsage();
        process.exit(1);
    }
    resourcesRootDir = path.resolve(resourcesRootDir);
    let extName = path.extname(inputFilePath).toLowerCase();

    if (extName === ".apk") {
        //1. copy apk to temp apk
        console.log("\nstep1/5: copy apk to temp apk================================");
        let tempApkFilePath = path.resolve("output/temp.apk");
        copyFile(inputFilePath, tempApkFilePath);

        //2. replace resources
        console.log("\nstep2/5: replace resources================================");
        if (!replaceResources(tempApkFilePath, resourcesRootDir))
            process.exit(1);

        //3. zipalign
        console.log("\nstep3/5: zipalign================================");
        let outputFilePath = createNewFilePath(inputFilePath, "_new");
        if (!zipAlign(config, tempApkFilePath, outputFilePath))
            process.exit(1);

        //4. sign with apksigner
        console.log("\nstep4/5: sign with apksigner================================");
        if (!signWithApksigner(config, outputFilePath))
            process.exit(1);

        //5. print cert
        console.log("\nstep5/5: print cert================================");
        printCertWithApksigner(config, outputFilePath);

        console.log("\n================================");
        console.log("Success: " + outputFilePath);
        console.log("================================");
    } else if (extName === ".aab") {
        //1. copy aab to temp aab
        console.log("\nstep1/6: copy aab to temp aab================================");
        let tempAabFilePath = path.resolve("output/temp.aab");
        copyFile(inputFilePath, tempAabFilePath);

        //2. replace resources
        console.log("\nstep2/6: replace resources================================");
        if (!replaceResources(tempAabFilePath, resourcesRootDir))
            process.exit(1);

        //3. unzip to temp dir
        console.log("\nstep3/6: unzip to temp dir================================");
        let tempDir = unzipTarget2Dir(tempAabFilePath, path.resolve("output/temp"));
        if (tempDir == null)
            process.exit(1);

        //4. remove signature files
        console.log("\nstep4/6: remove signature files================================");
        if (!removeSignatureFiles(tempDir))
            process.exit(1);

        //5. zip to new aab
        console.log("\nstep5/6: zip to new aab================================");
        let outputFilePath = createNewFilePath(inputFilePath, "_new");
        if (!zipDir2Target(tempDir, outputFilePath))
            process.exit(1);

        //6. sign with jarsigner
        console.log("\nstep6/6: sign with jarsigner================================");
        if (!signWithJarsigner(config, outputFilePath))
            process.exit(1);

        console.log("\nprint cert================================");
        printJarCert(outputFilePath);

        console.log("\n================================");
        console.log("Success: " + outputFilePath);
        console.log("================================");
    } else {
        console.error(`input APK/AAB/APKS,does't support ${input}`);
        process.exit(1);
    }
}

/**
 * Handles the aab2Apks command.
 * @param {Config} config validated tool configuration
 */
function cmdAab2Apks(config) {
    let input = getArg(5, "inputFilePath(aab)");
    if (input === "") {
        printUsage();
        process.exit(1);
    }
    let inputFilePath = path.resolve(input);
    let extName = path.extname(inputFilePath).toLowerCase();
    if (extName === ".aab") {
        //step1: convert aab to apks
        console.log("\nstep1/2: convert aab to apks================================");
        let outputFilePath = replaceExtName(inputFilePath, ".apks");
        if (!aab2Apks(config, inputFilePath, outputFilePath, false))
            process.exit(1);

        //step2: print cert
        console.log("\nstep2/2: print cert================================");
        printCertOfApksWithApkSigner(config, outputFilePath);

        console.log("\n================================");
        console.log("Success: " + outputFilePath);
        console.log("================================");
    } else {
        console.error(`input AAB, can't ${input}`);
        process.exit(1);
    }
}

/**
 * Handles the aab2Apk command and extracts universal.apk from the generated APKS.
 * @param {Config} config validated tool configuration
 */
function cmdAab2Apk(config) {
    let input = getArg(5, "inputFilePath(aab)");
    if (input === "") {
        printUsage();
        process.exit(1);
    }
    let inputFilePath = path.resolve(input);
    let extName = path.extname(inputFilePath).toLowerCase();
    if (extName === ".aab") {
        //step1: convert aab to apks
        console.log("\nstep1/4: convert aab to apks================================");
        let tempFilePath = path.resolve("output/temp.apks");
        if (!aab2Apks(config, inputFilePath, tempFilePath, true))
            process.exit(1);

        //step2: unzip apks to temp dir
        console.log("\nstep2/4: unzip apks to temp dir================================");
        let tempDir = unzipTarget2Dir(tempFilePath, path.resolve("output/temp"))
        if (tempDir == null)
            process.exit(1);

        //step3: copy universal apk to output apk
        console.log("\nstep3/4: copy universal apk to output apk================================");
        let universalApkFilePath = path.join(tempDir, "universal.apk");
        let outputFilePath = replaceExtName(inputFilePath, ".apk");
        copyFile(universalApkFilePath, outputFilePath, true);

        //step4: print cert
        console.log("\nstep4/4: print cert================================");
        printCertWithApksigner(config, outputFilePath);


        console.log("\n================================");
        console.log("Success: " + outputFilePath);
        console.log("================================");
    } else {
        console.error(`input AAB, does't support ${input}`);
        process.exit(1);
    }
}

/**
 * Handles the install command for APK, APKS, or AAB files.
 * @param {Config} config validated tool configuration
 */
function cmdInstall(config) {
    let input = getArg(5, "inputFilePath(apk/apks/aab)");
    if (input === "") {
        printUsage();
        process.exit(1);
    }
    let inputFilePath = path.resolve(input);
    let extName = path.extname(inputFilePath).toLowerCase();
    switch (extName) {
        default:
            console.error(`input APK/AAB/APKS, does't support ${input}`);
            process.exit(1);
            break;

        case ".apk":
            installApk(inputFilePath);

            break;

        case ".apks":
            installApks(config, inputFilePath);
            break;

        case ".aab":
            //1. convert aab to apks
            console.log("\nstep1/2: convert aab to apks================================");
            let outputFilePath = path.resolve("output/temp.apks");
            if (!aab2Apks(config, inputFilePath, outputFilePath, false))
                process.exit(1);

            //2. install apks
            console.log("\nstep2/2: install apks================================");
            if (!installApks(config, outputFilePath))
                process.exit(1);
            break;
    }
}

/**
 * Program entry point.
 */
function main() {
    let args = process.argv;

    //1. load config file
    if (args.length < 5 || args[2].toLowerCase() !== "-config") {
        printUsage()
        process.exit(1);
    }

    let configFilePath = args[3];
    const config = loadConfig(configFilePath);
    if (config == null || !validateConfig(config)) {
        process.exit(1);
    }

    //2. get command
    let cmd = args[4].toLocaleLowerCase();

    switch (cmd) {
        default:
            printUsage()
            process.exit(1);
            break;

        case "resign":
            cmdResign(config);
            break;

        case "replaceres":
            cmdReplaceResource(config);
            break;

        case "printcertself":
            printKeyStoreWithKeytool(config.keyStoreFilePath, config.keyStorePassword);
            break;

        case "printcert":
            cmdPrintCert(config);
            break;

        case "aab2apk":
            cmdAab2Apk(config);
            break;

        case "aab2apks":
            cmdAab2Apks(config);
            break;

        case "install":
            cmdInstall(config);
            break;
    }
}


if (require.main === module) {
    try {
        main();
    } catch (error) {
        console.error(`${error.message}`);
        process.exit(1);
    }
}

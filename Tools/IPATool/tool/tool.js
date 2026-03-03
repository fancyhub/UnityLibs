const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');
const os = require('os');
const zlib = require('zlib');

class ZipFile {
  constructor(filePath) {
    this.filePath = filePath;
    this.fd = fs.openSync(filePath, 'r');
    this.files = this._parseCentralDirectory();
  }

  // 获取所有文件名
  getAllFileNames() {
    return this.files.map(f => f.fileName);
  }

  // 获取文件 Buffer（自动流式读取）
  async getFile(fileName) {
    const entry = this.files.find(f => f.fileName === fileName);
    if (!entry)
      throw new Error(`File not found: ${fileName}`);

    return this._readFileData(entry);
  }

  // 获取文件 ReadStream（高级用法）
  getFileStream(fileName) {
    const entry = this.files.find(f => f.fileName === fileName);
    if (!entry)
      throw new Error(`File not found: ${fileName}`);

    const start = entry.dataStart;
    const end = start + entry.compressedSize - 1;

    const fileStream = fs.createReadStream(this.filePath, { start, end });

    if (entry.compression === 0)
      return fileStream;

    if (entry.compression === 8)
      return fileStream.pipe(zlib.createInflateRaw());

    throw new Error('Unsupported compression');
  }

  close() {
    fs.closeSync(this.fd);
  }

  getAppDirName() {
    const files = this.getAllFileNames();
    for (const filePath of files) {
      if (filePath.startsWith("Payload/") && filePath.endsWith(".app/")) {
        return path.basename(filePath);
      }
    }
    return null;
  }

  // ---------------- private ----------------

  _parseCentralDirectory() {
    const stat = fs.fstatSync(this.fd);
    const fileSize = stat.size;

    const readSize = Math.min(65536, fileSize);
    const buffer = Buffer.alloc(readSize);

    fs.readSync(
      this.fd,
      buffer,
      0,
      readSize,
      fileSize - readSize
    );

    const eocdSignature = 0x06054b50;
    let eocdOffset = -1;

    for (let i = readSize - 22; i >= 0; i--) {
      if (buffer.readUInt32LE(i) === eocdSignature) {
        eocdOffset = i;
        break;
      }
    }

    if (eocdOffset === -1)
      throw new Error('EOCD not found');

    const centralDirOffset = buffer.readUInt32LE(eocdOffset + 16);

    return this._readCentralDirectory(centralDirOffset);
  }

  _readCentralDirectory(offset) {
    const files = [];
    let ptr = offset;

    const header = Buffer.alloc(46);

    while (true) {
      fs.readSync(this.fd, header, 0, 46, ptr);

      if (header.readUInt32LE(0) !== 0x02014b50)
        break;

      const compression = header.readUInt16LE(10);
      const compressedSize = header.readUInt32LE(20);
      const localHeaderOffset = header.readUInt32LE(42);
      const fileNameLen = header.readUInt16LE(28);
      const extraLen = header.readUInt16LE(30);
      const commentLen = header.readUInt16LE(32);

      const nameBuffer = Buffer.alloc(fileNameLen);
      fs.readSync(this.fd, nameBuffer, 0, fileNameLen, ptr + 46);

      const fileName = nameBuffer.toString();

      const dataStart = this._getDataStart(localHeaderOffset);

      files.push({
        fileName,
        compression,
        compressedSize,
        localHeaderOffset,
        dataStart
      });

      ptr += 46 + fileNameLen + extraLen + commentLen;
    }

    return files;
  }

  _getDataStart(localHeaderOffset) {
    const header = Buffer.alloc(30);
    fs.readSync(this.fd, header, 0, 30, localHeaderOffset);

    if (header.readUInt32LE(0) !== 0x04034b50)
      throw new Error('Invalid local header');

    const fileNameLen = header.readUInt16LE(26);
    const extraLen = header.readUInt16LE(28);

    return localHeaderOffset + 30 + fileNameLen + extraLen;
  }

  _readFileData(entry) {
    return new Promise((resolve, reject) => {
      const chunks = [];

      const stream = this.getFileStream(entry.fileName);

      stream.on('data', chunk => chunks.push(chunk));
      stream.on('end', () => resolve(Buffer.concat(chunks)));
      stream.on('error', reject);
    });
  }
}


class Config {
  p12FilePath = '';
  p12Password = '';

  mobileprovisionFilePath = '';
}


/**
 * readLineFromStdin 
 * @param {string} name 
 * @returns {string}
 */
function readLineFromStdin(name) {
  process.stdout.write(`${name}:`);
  const buffer = Buffer.alloc(2048);
  const bytesRead = fs.readSync(process.stdin.fd, buffer, 0, buffer.length);
  const input = buffer.toString('utf8', 0, bytesRead).trim();
  return input;
}

/**
 * getArg
 * @param {number} index 
 * @param {string} name 
 * @returns {string}
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
 * loadConfig
 * @param {string} configFilePath 
 * @returns {Config} 
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
 * validate config
 * @param {Config} config 
 * @returns {boolean} true if config is valid, false otherwise
 */
function validateConfig(config) {
  if (config == null)
    return false

  config.p12FilePath = path.resolve(config.p12FilePath)
  // if (!fs.existsSync(config.p12FilePath)) {
  //   console.error("p12FilePath not exist: " + config.p12FilePath)
  //   return false
  // }

  config.mobileprovisionFilePath = path.resolve(config.mobileprovisionFilePath)
  // if (!fs.existsSync(config.mobileprovisionFilePath)) {
  //   console.error("mobileprovisionFilePath not exist: " + config.mobileprovisionFilePath)
  //   return false
  // }
  return true;
}


/**
 * 执行命令
 * @param {string[]} args 
 * @param {string} name 
 * @param {boolean} print 
 * @returns {boolean} true if command is successful, false otherwise
 */
function exeCmd(args, name, print) {
  const command = args.join(' ');
  try {
    if (print) {
      console.log(`Exe ${name} Command: \n\t${command}\n`);
    }
    execSync(command, { stdio: 'inherit', encoding: 'utf8' });
    return true;
  } catch (error) {
    console.error(`Exe ${name} Command Failed: ${error.message}`);
    return false;
  }
}

/**
 * clearDir
 * @param {string} dir 
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
 * 打印pem证书信息
 * @param {string} pemFilePath 
 */
function printPemInfoWithOpenSSL(pemFilePath) {
  try {

    //show text
    {
      const command = [
        `openssl x509 -noout `,
        `-in "${pemFilePath}"`,
        `-text`,
      ];
      exeCmd(command, "show text", false);
    }
    console.log("-----------------------------------------------------");
    //show md5
    {
      const command = [
        `openssl x509 -noout `,
        `-in "${pemFilePath}"`,
        `-fingerprint -md5`,
      ];
      exeCmd(command, "show md5", false);
    }

    //show sha1
    {
      const command = [
        `openssl x509 -noout `,
        `-in "${pemFilePath}"`,
        `-fingerprint -sha1`,
      ];
      exeCmd(command, "show sha1");
    }

    //show sha256
    {
      const command = [
        `openssl x509 -noout `,
        `-in "${pemFilePath}"`,
        `-fingerprint -sha256`,
      ];
      exeCmd(command, "show sha256");
    }

  } catch (err) {
    console.error('\n❌ 解析失败：');
    if (err.message.includes('password')) {
      console.error('→ 证书密码错误');
    } else {
      console.error('→', err.message);
    }
  }
}

/**
 * 纯原生 Node.js 解析 P12 证书并打印基础信息
 * @param {string} p12Path 证书路径
 * @param {string} password 证书密码
 */
function printP12InfoWithOpenSSL(p12Path, password) {
  try {

    let tempPemPath = path.resolve("output/temp.pem");
    if (fs.existsSync(tempPemPath)) {
      fs.unlinkSync(tempPemPath);
    }

    //p12 -> pem
    {
      const command = [
        `openssl`,
        `pkcs12`,
        `-legacy`,
        `-nokeys`,
        `-clcerts`,
        `-in "${p12Path}"`,
        `-passin pass:${password}`,
        `-out "${tempPemPath}"`,
      ];
      exeCmd(command, "p12 -> pem", false);
    }

    printPemInfoWithOpenSSL(tempPemPath);

  } catch (err) {
    console.error('\n❌ 解析失败：');
    if (err.message.includes('password')) {
      console.error('→ 证书密码错误');
    } else {
      console.error('→', err.message);
    }
  }
}

/**
 * 打印mobileprovision证书信息
 * @param {string} mobileprovisionFilePath 
 */
function printMobileProvisionInfoWithOpenSSL(mobileprovisionFilePath) {
  try {

    let tempPlistPath = path.resolve("output/temp.plist");
    if (fs.existsSync(tempPlistPath)) {
      fs.unlinkSync(tempPlistPath);
    }

    const command = [
      `openssl cms -inform DER -verify -noverify`,
      `-in "${mobileprovisionFilePath}"`,
      `-out "${tempPlistPath}"`,
    ];
    exeCmd(command, "mobileprovision -> plist & pem", true);

    //get pem from plist
    let tempPemPath = path.resolve("output/temp_pem_from_mobileprovision.pem");
    {
      if (fs.existsSync(tempPemPath)) {
        fs.unlinkSync(tempPemPath);
      }
      const xml = fs.readFileSync(tempPlistPath, 'utf8');

      // 提取 DeveloperCertificates 区块
      const blockMatch = xml.match(
        /<key>\s*DeveloperCertificates\s*<\/key>\s*<array>([\s\S]*?)<\/array>/
      );
      if (!blockMatch) {
        console.error('❌ DeveloperCertificates not found');
        process.exit(1);
      }


      // 提取所有 <data>...</data>
      const dataMatches = [...blockMatch[1].matchAll(/<data>([\s\S]*?)<\/data>/g)];

      if (dataMatches.length === 0) {
        console.error('❌ No certificates found');
        process.exit(1);
      }

      // console.log(`Found ${dataMatches.length} DeveloperCertificates`);

      dataMatches.forEach((m, index) => {
        console.log("DeveloperCertificates index: " + (index + 1) + "/ " + dataMatches.length);
        // 清理空白字符
        const base64 = m[1].replace(/\s+/g, '');

        // 转 PEM
        const pem =
          '-----BEGIN CERTIFICATE-----\n' +
          base64.match(/.{1,64}/g).join('\n') +
          '\n-----END CERTIFICATE-----\n';


        fs.writeFileSync(tempPemPath, pem);

        printPemInfoWithOpenSSL(tempPemPath);
      });
    }


  } catch (err) {
    console.error('→', err.message);
  }
}


function unzipIpa(srcIpaFilePath, destDirPath) {
  try {
    clearDir(destDirPath);
    const platform = os.platform();
    switch (platform) {
      case "win32":
        {
          const command = [
            `tar -xf "${srcIpaFilePath}" -C "${destDirPath}"`,
          ];
          exeCmd(command, "unzip ipa", false);
        }
        break;

      default:
        {
          const command = [
            `unzip -o "${srcIpaFilePath}" -d "${destDirPath}"`,
          ];
          exeCmd(command, "unzip ipa", false);
        }
        break;
    }
  } catch (err) {
    console.error('→', err.message);
  }
}

function findIPAAppDir(ipaDirPath) {
  let payloadDirPath = path.resolve(ipaDirPath, "Payload");
  if (!fs.existsSync(payloadDirPath)) {
    return null;
  }

  let payloadDirPaths = fs.readdirSync(payloadDirPath);
  for (const appDirPath of payloadDirPaths) {
    if (appDirPath.endsWith(".app")) {
      return path.resolve(payloadDirPath, appDirPath);
    }
  }
  return null;
}

async function printIpaInfoWithOpenSSL(ipaFilePath) {
  try {
    const zipFile = new ZipFile(ipaFilePath);
    const appDirName = zipFile.getAppDirName();
    const srcFilePath = "Payload/" + appDirName + "/embedded.mobileprovision";

    const destFilePath = path.resolve("output", "embedded.mobileprovision");
    if (fs.existsSync(destFilePath)) {
      fs.unlinkSync(destFilePath);
    }

    const fileData = await zipFile.getFile(srcFilePath);
    fs.writeFileSync(destFilePath, fileData);

    if (!fs.existsSync(destFilePath)) {
      console.error('❌ mobileprovision not found');
      process.exit(1);
    }
    printMobileProvisionInfoWithOpenSSL(destFilePath);
  }
  catch (err) {
    console.error('→', err.message);
  }
}


/**
 * cmdPrintCert
 */
function cmdPrintCert() {
  let input = getArg(3, "inputFilePath(p12/pem/mobileprovision/ipa)")
  if (input === "") {
    printUsage();
    process.exit(1);
  }
  let inputFilePath = path.resolve(input);
  let extName = path.extname(inputFilePath).toLowerCase();
  // console.log("inputFilePath: " + inputFilePath + ", extName: " + extName);
  switch (extName) {
    default:
      console.error(`input p12/pem/mobileprovision/ipa, does't support ${input}`);
      process.exit(1);
      break;

    case ".p12":
      let password = getArg(6, "password")
      printP12InfoWithOpenSSL(inputFilePath, password);
      break

    case ".pem":
      printPemInfoWithOpenSSL(inputFilePath);
      break;

    case ".mobileprovision":
      printMobileProvisionInfoWithOpenSSL(inputFilePath);
      break;

    case ".ipa":
      printIpaInfoWithOpenSSL(inputFilePath);
      break;
  }
}

function printUsage() {
  let selfName = path.basename(__filename)

  console.log(`
Usage: node ${selfName} cmd 
  cmd:
      - printCert: print the cert info about the p12, 
          example: node ${selfName} printCert inputFilePath           
      - convertPlist: convert the (bplist/ipa) to plist
          example: node ${selfName} convertPlist inputFilePath
      `);
}


function convertBplist2Plist(inputFilePath, outputFilePath) {
  const platform = os.platform();
  switch (platform) {
    case "darwin":
      {
        const command = [
          `plutil -convert xml1`,
          `"${inputFilePath}"`
            `-o "${outputFilePath}"`,
        ];
        exeCmd(command, "convert plist to xml", false);
      }
      break;

    case "win32":
      {
        const plistutilPath = path.resolve("tool/plistutil.exe");
        const command = [
          `${plistutilPath}`,
          `-i "${inputFilePath}"`,
          `-o "${outputFilePath}"`,
        ];
        exeCmd(command, "convert plist to xml", false);
      }
      break;

    default:
      {
        const command = [
          `plistutil`,
          `-i "${inputFilePath}"`,
          `-o "${outputFilePath}"`,
        ];
        exeCmd(command, "convert plist to xml", false);
      }
      break;
  }
}

async function cmdConvertPlist() {

  let input = getArg(3, "inputFilePath(plist/ipa)")
  if (input === "") {
    printUsage();
    process.exit(1);
  }


  try {
    let inputFilePath = path.resolve(input);
    let extName = path.extname(inputFilePath).toLowerCase();
    let outputFilePath = inputFilePath + ".plist";

    switch (extName) {
      case ".plist":
        convertBplist2Plist(inputFilePath, outputFilePath);
        break;

      case ".ipa":
        {
          const zipFile = new ZipFile(inputFilePath);
          const appDirName = zipFile.getAppDirName();
          const srcFilePath = "Payload/" + appDirName + "/Info.plist";
          const destFilePath = path.resolve("output", "Info.plist");
          if (fs.existsSync(destFilePath)) {
            fs.unlinkSync(destFilePath);
          }
          const fileData = await zipFile.getFile(srcFilePath);
          fs.writeFileSync(destFilePath, fileData);
          convertBplist2Plist(destFilePath, outputFilePath);
        }
        break;

      default:
        console.error(`input plist/ipa, does't support ${extName}`);
        process.exit(1);
        break;
    }

    console.log("convert plist to xml success: " + outputFilePath);
  } catch (err) {
    console.error('→', err.message);
  }
}
 

function main(args) {

  //1. load config file
  if (args.length < 3) {
    printUsage()
    process.exit(1);
  }

  //2. get command   
  let cmd = args[2].toLocaleLowerCase();

  switch (cmd) {
    default:
      printUsage()
      process.exit(1);
      break;

    case "printcert":
      cmdPrintCert();
      break;


    case "convertplist":
      cmdConvertPlist();
      break;
  }
}



if (require.main === module) {
  try {
    // process.argv = ["node", "tool.js", "convertPlist", "C:\\Users\\cunyu.fan\\Desktop\\KD_ios_Dev_1053759_0.0.2443_ec_appVer2.1_debuggable_34064_ad-hoc.ipa"];
    main(process.argv);
  } catch (error) {
    console.error(`${error.message}`);
    process.exit(1);
  }
}


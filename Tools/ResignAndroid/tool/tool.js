const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

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
 * signWithJarsigner https://docs.oracle.com/javase/8/docs/technotes/tools/windows/jarsigner.html
 * @param {Config} config 
 * @param {string} filePath 
 * @returns {boolean} true if success, false otherwise
 */
function signWithJarsigner(config, filePath) {
    const command = [
        `jarsigner`,
        `-keystore "${config.keyStoreFilePath}"`,
        `-storepass ${config.keyStorePassword}`,
        `-keypass "${config.keyPassword}"`,
        `-sigalg SHA256withRSA`,
        `-digestalg SHA-256`,
        `"${filePath}"`,
        `"${config.keyAlias}"`
    ].join(' ');

    try {
        console.log(`Exe Sign Command: $\n\t${command}\n`);
        execSync(command, { stdio: 'inherit' });
        return true;
    } catch (error) {
        console.error(`Sign Failed: ${error.message}`);
        return false;
    }
}


/**
 * signWithApksigner https://developer.android.com/tools/apksigner
 * @param {Config} config 
 * @param {string} filePath 
 * @returns {boolean} true if success, false otherwise
 */
function signWithApksigner(config, filePath) {
    const command = [
        `java -jar "${config.apksignerPath}"`,
        `sign`,
        `--ks "${config.keyStoreFilePath}"`,
        `--ks-pass pass:${config.keyStorePassword}`,
        `--ks-key-alias "${config.keyAlias}"`,
        `--key-pass pass:${config.keyPassword}`,
        `--v1-signing-enabled true`,
        `--v2-signing-enabled true`,
        `--v3-signing-enabled true`,
        `--v4-signing-enabled true`,
        `"${filePath}"`,
    ].join(' ');

    try {
        console.log(`Exe Sign Command:\n\t${command}\n`);
        execSync(command, { stdio: 'inherit' });
        return true;
    } catch (error) {
        console.error(`Sign Failed: ${error.message}`);
        return false;
    }
}

/**
 * zipAlign https://developer.android.com/tools/zipalign
 * @param {Config} config 
 * @param {string} inputFilePath 
 * @param {string} outputFilePath 
 * @returns {boolean} true if success, false otherwise
 */
function zipAlign(config, inputFilePath, outputFilePath) {
    const command = [
        `"${config.zipalignPath}"`,
        `-P 16`,
        `-f`,
        // `-v`,
        `4`,
        `"${inputFilePath}"`,
        `"${outputFilePath}"`
    ].join(' ');

    try {
        console.log(`Exe zipalign: \n\t${command}\n`);
        execSync(command, { stdio: 'inherit' });
        return true;
    } catch (error) {
        console.error(`zipalign Failed: ${error.message}`);
        return false;
    }
}

function installApk(inputFilePath) {
    const command = [
        `adb install -r "${inputFilePath}"`,
    ].join(' ');

    try {

        console.log(`Exe installApk: \n\t${command}\n`);
        execSync(command, { stdio: 'inherit' });
        return true;
    } catch (error) {
        console.error(`Exe installApk Failed: ${error.message}`);
        return false;
    }
}


function installApks(config, inputFilePath) {
    const command = [
        `java -jar "${config.bundletoolPath}"`,
        `install-apks`,
        `--apks="${inputFilePath}"`,
    ].join(' ');

    try {

        console.log(`Exe installApks: \n\t${command}\n`);
        execSync(command, { stdio: 'inherit' });
        return true;
    } catch (error) {
        console.error(`Exe installApks Failed: ${error.message}`);
        return false;
    }
}


/**
 * replaceResources
 * @param {string} apkFilePath 
 * @param {string} resourcesRootDir 
 * @returns {boolean} true if success, false otherwise
 */
function replaceResources(apkFilePath, resourcesRootDir) {
    const command = [
        `jar -uf`,
        `"${apkFilePath}"`,
        `-C "${resourcesRootDir}"`,
        `./`
    ].join(' ');

    try {
        console.log(`Exe replaceResources: \n\t${command}\n`);
        execSync(command, { stdio: 'inherit', cwd: resourcesRootDir });
        return true;
    } catch (error) {
        console.error(`Exe replaceResources: ${error.message}`);
        return false;
    }
}

/**
 * printCertWithApksinger https://developer.android.com/tools/apksigne
 * @param {Config} config 
 * @param {string} inputFilePath apk,apks
 * @returns {boolean} true if success, false otherwise
 */
function printCertWithApksinger(config, inputFilePath) {
    const command = [
        `java -jar`,
        `"${config.apksignerPath}"`,
        `verify`,
        `--verbose`,
        `--print-certs`,
        `"${inputFilePath}"`,
    ].join(' ');

    try {
        console.log(`print cert Of Apk:\n\t${command}\n`);
        execSync(command, { stdio: 'inherit' });
        return true;
    } catch (error) {
        console.error(`print cert Of Apk Failed: ${error.message}`);
        return false;
    }
}

/**
 * aab2Apks https://developer.android.com/tools/bundletool
 * @param {Config} config 
 * @param {string} aabFilePath 
 * @param {string} apksFilePath 
 * @param {boolean} universal 
 * @returns {boolean} true if success, false otherwise
 */
function aab2Apks(config, aabFilePath, apksFilePath, universal) {
    let mode = "";
    if (universal) {
        mode = "--mode=universal";
    }
    const command = [
        `java -jar "${config.bundletoolPath}"`,
        `build-apks`,
        `${mode}`,
        `--bundle="${aabFilePath}"`,
        `--output="${apksFilePath}"`,
        `--ks "${config.keyStoreFilePath}"`,
        `--ks-pass pass:${config.keyStorePassword}`,
        `--ks-key-alias "${config.keyAlias}"`,
        `--key-pass pass:${config.keyPassword}`,
        `--overwrite`
    ].join(' ');

    try {
        console.log(`aab2Apks Command:\n\t${command}\n`);
        execSync(command, { stdio: 'inherit' });
        return true;
    } catch (error) {
        console.error(`aab2Apks Failed: ${error.message}`);
        return false;
    }
}


/**
 * printJarCert https://docs.oracle.com/javase/8/docs/technotes/tools/windows/jarsigner.html
 * @param {string} inputFilePath aab,jar
 * @returns {boolean} true if success, false otherwise
 */
function printJarCert(inputFilePath) {
    const command = [
        `jarsigner`,
        `-verify`,
        `-verbose:signing`,
        `-certs`,
        `"${inputFilePath}"`,
    ].join(' ');

    try {
        console.log(`print cert with jarsigner:\n\t${command}\n`);
        execSync(command, { stdio: 'inherit' });
    } catch (error) {
        console.error(`print cert with jarsigner Failed: ${error.message}`);
        return false;
    }

    unzipTargetMetaInf2Dir(inputFilePath, path.resolve("output/temp"));

    let metaInfDir = path.resolve("output/temp/META-INF/");
    if (!fs.existsSync(metaInfDir)) {
        return;
    }

    const fileNames = fs.readdirSync(metaInfDir);
    for (const fileName of fileNames) {
        extName = path.extname(fileName)
        if (extName === ".RSA" || extName === ".DSA") {
            const certFilePath = path.resolve(metaInfDir, fileName);
            printCertWithKeytool(certFilePath);
        }
    }
}

/**
 * printKeyStoreWithKeytool
 * @param {string} keyStoreFilePath  keystore file path
 * @param {string} storePassword  store password
 * @returns {boolean} true if success, false otherwise
 */
function printKeyStoreWithKeytool(keyStoreFilePath, storePassword) {
    const command = [
        `keytool`,
        `-list`,
        `-v`,
        `-keystore "${keyStoreFilePath}"`,
        `-storepass "${storePassword}"`,
    ].join(' ');

    try {
        console.log(`print keystore with keytool: \n\t${command}\n`);
        execSync(command, { stdio: 'inherit' });
        return true;
    } catch (error) {
        console.error(`print keystore with keytool Failed: ${error.message}`);
        return false;
    }
}


/**
 * printCertWithKeytool
 * @param {string} keyStoreFilePath  keystore file path
 * @param {string} storePassword  store password
 * @returns {boolean} true if success, false otherwise
 */
function printCertWithKeytool(certFilePath) {
    const command = [
        `keytool`,
        `-printcert`,
        `-file  "${certFilePath}"`,
    ].join(' ');

    try {
        console.log(`print cert with keytool: \n\t${command}\n`);
        execSync(command, { stdio: 'inherit' });
        return true;
    } catch (error) {
        console.error(`print cert with keytool Failed: ${error.message}`);
        return false;
    }
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
 * createNewFilePath
 * @param {string} inputFilePath 
 * @param {string} appendSuffix 
 * @returns {string} result new file path
 */
function createNewFilePath(inputFilePath, appendSuffix) {
    let extName = path.extname(inputFilePath);
    return inputFilePath.substring(0, inputFilePath.length - extName.length) + appendSuffix + extName;
}

/**
 * replaceExtName
 * @param {string} inputFilePath 
 * @param {string} appendSuffix 
 * @returns {string} result new file path
 */
function replaceExtName(inputFilePath, newExtName) {
    let extName = path.extname(inputFilePath);
    return inputFilePath.substring(0, inputFilePath.length - extName.length) + newExtName;
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
 * copyFile
 * @param {string} sourceFilePath 
 * @param {string} targetFilePath 
 * @param {boolean} changeTime 
 */
function copyFile(sourceFilePath, targetFilePath, changeTime = false) {
    console.log("\tCopy File: " + sourceFilePath + " -> " + targetFilePath);
    fs.copyFileSync(sourceFilePath, targetFilePath);

    if (changeTime) {
        let time = new Date();
        fs.utimesSync(targetFilePath, time, time);
        // fs.utimesSync(targetFilePath, fs.statSync(sourceFilePath).atime, fs.statSync(sourceFilePath).mtime);
    }
}

/**
 * unzip 
 * @param {string} inputFilePath 
 * @param {string} baseDir 
 * @returns {bool} 
 */
function unzipTargetMetaInf2Dir(inputFilePath, baseDir) {
    baseDir = path.resolve(baseDir);

    try {
        clearDir(baseDir);

        const command = [
            `jar`,
            `-xf`,
            `"${inputFilePath}"`,
            'META-INF/'
        ].join(' ');

        console.log(`unzipTargetMetaInf2Dir: \n\t${command}\n`);
        execSync(command, { stdio: 'inherit', cwd: baseDir });
        return true;
    } catch (error) {
        console.error(`unzipTargetMetaInf2Dir Failed: ${error.message}`);
        return false;
    }
}

/**
 * unzip 
 * @param {string} inputFilePath 
 * @param {string} baseDir 
 * @returns {bool} 
 */
function unzipTarget2Dir(inputFilePath, baseDir) {
    try {
        clearDir(baseDir);

        tempDir = path.resolve(tempDir);
        const command = [
            `jar`,
            `-xf`,
            `"${inputFilePath}"`,
            './'
        ].join(' ');

        console.log(`unzip2TempDir: \n\t${command}\n`);
        execSync(command, { stdio: 'inherit', cwd: baseDir });
        return true;
    } catch (error) {
        console.error(`unzip2TempDir Failed: ${error.message}`);
        return false;
    }
}

/**
 * zip dir to target
 * @param {string} inputDir 
 * @param {string} outputFilePath 
 * @returns {boolean} true if success, false otherwise
 */
function zipDir2Target(inputDir, outputFilePath) {
    const command = [
        `jar`,
        `-cf`,
        `"${outputFilePath}"`,
        '.'
    ].join(' ');

    try {
        console.log(`zipDir2Target: \n\t${command}\n`);
        execSync(command, { stdio: 'inherit', cwd: inputDir });
        return true;
    } catch (error) {
        console.error(`zipDir2Target: ${error.message}`);
        return false;
    }
}


/**
 * removeSignatureFiles 
 * @param {string} baseDir 
 * @returns {boolean} true if success, false otherwise
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


function readLineFromStdin(name) {
    process.stdout.write(`${name}:`);
    const buffer = Buffer.alloc(2048);
    const bytesRead = fs.readSync(process.stdin.fd, buffer, 0, buffer.length);
    const input = buffer.toString('utf8', 0, bytesRead).trim();
    return input;
}

/**
 * 
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
            example: node ${selfName} -config </path/xxx/config.json> adbInstall inputFilePath
        `);
}

function main() {
    let args = process.argv;

    //1. load config file
    if (args.length < 4 || args[2].toLowerCase() !== "-config") {
        printUsage()
        process.exit(1);
    }

    let configFilePath = args[3];
    const config = loadConfig(configFilePath);
    if (config == null || !validateConfig(config)) {
        process.exit(1);
    }

    //2. get command
    if (args.length < 5) {
        printUsage()
        process.exit(1);
    }
    let cmd = args[4].toLocaleLowerCase();


    switch (cmd) {
        default:
            printUsage()
            process.exit(1);
            break;

        case "resign":
            {
                let input = getArg(5, "inputFilePath(apk/aab)");
                if (input === "") {
                    printUsage();
                    process.exit(1);
                }

                let inputFilePath = path.resolve(input);
                let extName = path.extname(inputFilePath).toLowerCase();
                switch (extName) {
                    default:
                        console.error(`input apk/aab,does't support ${input}`);
                        process.exit(1);
                        break;

                    case ".apk":
                        {
                            //step1: copy apk to temp apk
                            console.log("\nstep1/3: copy apk to temp apk================================");
                            let outputFilePath = createNewFilePath(inputFilePath, "_new");
                            copyFile(inputFilePath, outputFilePath);

                            //step2: sign with apksigner
                            console.log("\nstep2/3: sign with apksigner================================");
                            if (!signWithApksigner(config, outputFilePath))
                                return;

                            //step3: print cert
                            console.log("\nstep3/3: print cert================================");
                            printCertWithApksinger(config, outputFilePath);

                            console.log("\n================================");
                            console.log("Success: " + outputFilePath);
                            console.log("================================");
                        }
                        break;

                    case ".aab":
                        {
                            //1. unzip to temp dir
                            console.log("\nstep1/5: unzip to temp dir================================");
                            let tempDir = path.resolve("output/temp");
                            if (!unzipTarget2Dir(inputFilePath, tempDir))
                                return;

                            //2. remove signature files
                            console.log("\nstep2/5: remove signature files================================");
                            if (!removeSignatureFiles(tempDir))
                                return;

                            //3. zip to new aab
                            console.log("\nstep3/5: zip to new aab================================");
                            let outputFilePath = createNewFilePath(inputFilePath, "_new");
                            if (!zipDir2Target(tempDir, outputFilePath))
                                return;

                            //4. sign with apksigner
                            console.log("\nstep4/5: sign with jarsigner================================");
                            if (!signWithJarsigner(config, outputFilePath))
                                return;

                            //5. print cert
                            console.log("\nstep5/5: print cert================================");
                            printJarCert(outputFilePath);

                            console.log("\n================================");
                            console.log("Success: " + outputFilePath);
                            console.log("================================");
                        }
                        break;

                }
            }

            break;

        case "replaceRes":
            {
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

                switch (extName) {
                    default:
                        console.error(`input APK/AAB/APKS,does't support ${input}`);
                        process.exit(1);
                        break;

                    case ".apk":
                        {
                            //1. copy apk to temp apk
                            console.log("\nstep1/5: copy apk to temp apk================================");
                            let tempApkFilePath = path.resolve("output/temp.apk");
                            copyFile(inputFilePath, tempApkFilePath);

                            //2. replace resources
                            console.log("\nstep2/5: replace resources================================");
                            if (!replaceResources(tempApkFilePath, resourcesRootDir))
                                return;

                            //3. zipalign
                            console.log("\nstep3/5: zipalign================================");
                            let outputFilePath = createNewFilePath(inputFilePath, "_new");
                            if (!zipAlign(config, tempApkFilePath, outputFilePath))
                                return;

                            //4. sign with apksigner
                            console.log("\nstep4/5: sign with apksigner================================");
                            if (!signWithApksigner(config, outputFilePath))
                                return;

                            //5. print cert
                            console.log("\nstep5/5: print cert================================");
                            printCertWithApksinger(config, outputFilePath);

                            console.log("\n================================");
                            console.log("Success: " + outputFilePath);
                            console.log("================================");
                        }
                        break;

                    case ".aab":
                        {
                            //1. copy apk to temp apk
                            console.log("\nstep1/4: copy aab to temp aab================================");
                            let outputFilePath = createNewFilePath(inputFilePath, "_new");
                            copyFile(inputFilePath, outputFilePath);

                            //2. replace resources
                            console.log("\nstep2/4: replace resources================================");
                            if (!replaceResources(outputFilePath, resourcesRootDir))
                                return;

                            //3. sign with jarsigner
                            console.log("\nstep3/4: sign with jarsigner================================");
                            if (!signWithJarsigner(config, outputFilePath))
                                return;

                            //4. print cert
                            console.log("\nstep4/4: print cert================================");
                            printJarCert(outputFilePath);

                            console.log("\n================================");
                            console.log("Success: " + outputFilePath);
                            console.log("================================");
                        }
                        break;

                }
            }
            break;

        case "printcertself":
            {
                printKeyStoreWithKeytool(config.keyStoreFilePath, config.keyStorePassword);
            }
            break;

        case "printcert":
            {
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
                        printCertWithApksinger(config, inputFilePath);
                        break;

                    case ".apks":
                    case ".aab":
                        printJarCert(inputFilePath);
                        break;

                    case ".keystore":
                        let password = getArg(6, "password")                         
                        printKeyStoreWithKeytool(inputFilePath, password);
                        break

                }
            }
            break;
        case "aab2apk":
            {
                let input = getArg(5, "inputFilePath(aab)");
                if (input === "") {
                    printUsage();
                    process.exit(1);
                }
                let inputFilePath = path.resolve(input);
                let extName = path.extname(inputFilePath).toLowerCase();
                switch (extName) {
                    default:
                        console.error(`input AAB, does't support ${input}`);
                        process.exit(1);
                        break;

                    case ".aab":
                        {
                            //step1: convert aab to apks
                            console.log("\nstep1/4: convert aab to apks================================");
                            let tempFilePath = path.resolve("output/temp.apks");
                            if (!aab2Apks(config, inputFilePath, tempFilePath, true))
                                return;

                            //step2: unzip apks to temp dir
                            console.log("\nstep2/4: unzip apks to temp dir================================");
                            let tempDir = unzipTarget2Dir(tempFilePath)
                            if (tempDir == null)
                                return;

                            //step3: copy universal apk to output apk
                            console.log("\nstep3/4: copy universal apk to output apk================================");
                            let universalApkFilePath = path.join(tempDir, "universal.apk");
                            let outputFilePath = replaceExtName(inputFilePath, ".apk");
                            copyFile(universalApkFilePath, outputFilePath, true);

                            //step4: print cert
                            console.log("\nstep4/4: print cert================================");
                            printCertWithApksinger(config, outputFilePath);


                            console.log("\n================================");
                            console.log("Success: " + outputFilePath);
                            console.log("================================");
                        }
                        break;
                }
            }
            break;

        case "aab2apks":
            let input = getArg(5, "inputFilePath(aab)");
            if (input === "") {
                printUsage();
                process.exit(1);
            }
            let inputFilePath = path.resolve(input);
            let extName = path.extname(inputFilePath).toLowerCase();
            switch (extName) {
                default:
                    console.error(`input AAB, can't ${input}`);
                    process.exit(1);
                    break;

                case ".aab":
                    {
                        //step1: convert aab to apks
                        console.log("\nstep1/2: convert aab to apks================================");
                        let outputFilePath = replaceExtName(inputFilePath, ".apks");
                        if (!aab2Apks(config, inputFilePath, outputFilePath, false))
                            return;

                        //step2: print cert
                        console.log("\nstep2/2: print cert================================");
                        printJarCert(outputFilePath);

                        console.log("\n================================");
                        console.log("Success: " + outputFilePath);
                        console.log("================================");
                    }
                    break;
            }
            break;

        case "install":
            {
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
                        {
                            if (!installApk(inputFilePath))
                                return;
                        }
                        break;

                    case ".apks":
                        {
                            if (!installApks(config, inputFilePath))
                                return;
                        }
                        break;

                    case ".aab":
                        {
                            //1. convert aab to apks
                            console.log("\nstep1/2: convert aab to apks================================");
                            let outputFilePath = path.resolve("output/temp.apks");
                            if (!aab2Apks(config, inputFilePath, outputFilePath, false))
                                return;

                            //2. install apks
                            console.log("\nstep2/2: install apks================================");
                            if (!installApks(config, outputFilePath))
                                return;
                        }
                        break;
                }
            }
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
const activeScanners = {};

function createNew(key, scannerOptions, verbose) {
    if (activeScanners[key]) {
        throw new Error('A scanner already exists for the given key.');
    }

    return window.zbar.ZBarScanner.create().then(function (scanner) {
        activeScanners[key] = scanner;

        scannerOptions.forEach(function (symbolOption) {
            const symbolType = window.zbar.ZBarSymbolType[symbolOption.symbolType];

            symbolOption.configOptions.forEach(function (configOption) {
                const configType = window.zbar.ZBarConfigType[configOption.configType];
                const result = scanner.setConfig(symbolType, configType, configOption.value);

                if (verbose) {
                    console.log('Set ' + window.zbar.ZBarSymbolType[symbolType] + ' w/ ' + window.zbar.ZBarConfigType[configType] + ' to ' + configOption.value + ' with result ' + result);
                }
            });
        });
    });
}

function destroy(key) {
    const scanner = activeScanners[key];
    if (scanner) {
        scanner.destroy();
        delete activeScanners[key];
    }
}

function scan(key, imageData, verbose) {
    const scanner = activeScanners[key];
    if (scanner) {
        return window.zbar.scanImageData(imageData, scanner).then(function (symbols) {
            const results = [];
            symbols.forEach(function (symbol) {
                symbol.rawValue = symbol.decode();
                results.push(symbol);
                if (verbose) {
                    console.log(symbol);
                }
            });

            return results;
        });
    } else {
        return Promise.resolve([]);
    }
}

export default { createNew, destroy, scan };
import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';

var aspnetFolder;

if (env.APPDATA !== undefined && env.APPDATA !== '') {
    aspnetFolder = path.join(env.APPDATA, "ASP.NET")
}
else if (env.HOME !== undefined && env.Home !== '') {
    aspnetFolder = path.join(env?.HOME, ".aspnet")
}
else {
    throw new Error("Neither Home or Appdata is set");
}

if (!fs.existsSync(aspnetFolder)) {
    fs.mkdirSync(aspnetFolder);
}

const baseFolder = path.join(aspnetFolder, "https");

if (!fs.existsSync(baseFolder)) {
    fs.mkdirSync(baseFolder);
}

const certificateName = "dayzservermanager.client";
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    if (0 !== child_process.spawnSync('dotnet', [
        'dev-certs',
        'https',
        '--export-path',
        certFilePath,
        '--format',
        'Pem',
        '--no-password',
    ], { stdio: 'inherit', }).status) {
        throw new Error("Could not create certificate.");
    }
}

const target = env.ASPNETCORE_HTTPS_PORT ? `http://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
    env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:5172';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    server: {
        proxy: {
            '/DayZServer': {
                target,
                secure: false
            },
            '/ManagerConfig': {
                target,
                secure: false
            },
            '/ServerConfig': {
                target,
                secure: false
            },
            '/SchedulerConfig': {
                target,
                secure: false
            },
            '/RarityEditor': {
                target,
                secure: false
            }
        },
        port: 5173,
        https: {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath),
        },
        host: true
    },
    optimizeDeps: {
        exclude: ['mui_x-data-grid.js']
    }
})

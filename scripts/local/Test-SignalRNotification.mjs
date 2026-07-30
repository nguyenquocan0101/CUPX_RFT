import { createRequire } from 'node:module';
import { readFile } from 'node:fs/promises';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const repoRoot = resolve(dirname(fileURLToPath(import.meta.url)), '..', '..');
const baseUrl = process.env.LOCAL_MAIN_API_URL || 'http://localhost:5100';
const varsPath = resolve(repoRoot, '.local', 'main-api-vars');
const frontendNodeModules = resolve(repoRoot, 'AutomaticBrewingCoffeeFE', 'node_modules');
const require = createRequire(import.meta.url);

function readLocalVars(text) {
  const values = {};
  for (const line of text.split(/\r?\n/)) {
    const index = line.indexOf('=');
    if (index > 0 && !line.trimStart().startsWith('#')) {
      values[line.slice(0, index).trim()] = line.slice(index + 1);
    }
  }
  return values;
}

async function request(path, options = {}) {
  const response = await fetch(`${baseUrl}${path}`, options);
  const text = await response.text();
  let body = null;
  try {
    body = text ? JSON.parse(text) : null;
  } catch {
    body = text;
  }
  if (!response.ok) {
    throw new Error(`${options.method || 'GET'} ${path} returned HTTP ${response.status}`);
  }
  return body;
}

const values = readLocalVars(await readFile(varsPath, 'utf8'));
const jsonHeaders = { 'Content-Type': 'application/json' };
const kioskHeaders = {
  ...jsonHeaders,
  'X-API-Key': values.LocalSeed__KioskApiKey,
};

const login = await request('/api/v1/auth/login', {
  method: 'POST',
  headers: jsonHeaders,
  body: JSON.stringify({
    email: values.LocalSeed__AdminEmail,
    password: values.LocalSeed__AdminPassword,
  }),
});
const token = login?.response?.accessToken;
if (!token) throw new Error('Local login did not return a JWT.');

const order = await request('/api/v1/orders', {
  method: 'POST',
  headers: kioskHeaders,
  body: JSON.stringify({
    kioskId: 'local-kiosk',
    content: 'local-signalr-notification',
    clientId: 'local-signalr-test',
    paymentGateway: 'RESO',
    orderDetails: [{
      productId: 'local-product',
      productName: 'Local Coffee',
      productDescription: 'Local development product',
      quantity: 1,
      sellingPrice: 20000,
      productAttributes: [],
    }],
  }),
});
const orderId = order?.response?.orderId;
if (!orderId) throw new Error('Local order creation did not return an order ID.');

const signalR = require(resolve(frontendNodeModules, '@microsoft', 'signalr'));
const WebSocket = require(resolve(frontendNodeModules, 'ws'));
globalThis.WebSocket = WebSocket;

const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${baseUrl}/hubs/notification`, {
    accessTokenFactory: () => token,
    transport: signalR.HttpTransportType.WebSockets,
    skipNegotiation: true,
  })
  .build();

const notification = new Promise((resolveNotification, rejectNotification) => {
  const timeout = setTimeout(() => rejectNotification(new Error('Timed out waiting for ReceiveNotification.')), 15000);
  connection.on('ReceiveNotification', (payload) => {
    clearTimeout(timeout);
    resolveNotification(payload);
  });
});

try {
  await connection.start();
  await request('/api/v1/orders/fail', {
    method: 'PUT',
    headers: kioskHeaders,
    body: JSON.stringify({
      orderId,
      status: 'Failed',
      message: 'local-signalr-notification',
      finishedProductIds: [],
      failedProductIds: ['local-product'],
      preparingProductIds: [],
    }),
  });

  const payload = await notification;
  if (!payload || !payload.notificationId) {
    throw new Error('ReceiveNotification payload did not contain notificationId.');
  }
  console.log(`SignalR notification E2E passed: order=${orderId}`);
} finally {
  await connection.stop();
}

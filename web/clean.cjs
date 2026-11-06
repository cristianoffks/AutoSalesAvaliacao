const fs = require('node:fs');
const path = require('node:path');

for (const folder of ['node_modules', '.angular', 'dist']) {
  fs.rmSync(path.join(__dirname, folder), { recursive: true, force: true });
}

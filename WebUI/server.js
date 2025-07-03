const express = require('express');
const path = require('path');

const app = express();
const PORT = 3000;

app.use(express.static(path.join(__dirname, 'public')));

app.listen(PORT, () => {
    console.log(`WebUI is running at http://localhost:${PORT}`);
});
// This code sets up a simple Express server that serves static files from the 'public' directory.
// The server listens on port 3000 and logs a message to the console when it starts successfully.
// Make sure to create a 'public' directory with your HTML, CSS, and JavaScript files in the same directory as this server.js file.
// You can run this server using Node.js by executing `node server.js` in your terminal or command prompt.
// Ensure you have Express installed in your project by running `npm install express` before starting the server.
// This server can be used to serve a web interface for your application, allowing users to interact with it through a web browser.
// You can access the web interface by navigating to http://localhost:3000 in your web browser.
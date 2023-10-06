# CoreMod React UI

React frontend for the CoreMod Discord moderation bot project.

## Development

### Prerequisites

- [Node.js](https://nodejs.org/en/)

### Setup

1. Install dependencies: `npm install`
2. Start development server: `npm run dev`
3. Open <http://localhost:3000> in your browser

Vite will automatically proxy requests to the backend server running on port 5045.

### Making API changes

The API client is generated using [openapi-typescript-codegen](https://github.com/ferdikoomen/openapi-typescript-codegen). When making changes to the API, run `npm run generate-client` to update the client code. The backend server must be running on port 5045 for this to work.

**Do not edit the generated code directly.** Changes will be overwritten the next time the client is generated.

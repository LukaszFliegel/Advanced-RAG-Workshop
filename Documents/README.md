# Documents Folder

Place your PDF documents in this folder for the RAG workshop demo.

The application will automatically process all `.pdf` files in this directory and create embeddings for them.

## Sample Documents

For the workshop, you can add sample PDF documents such as:
- Company documentation
- Product manuals
- Research papers
- Any text-based PDF content

The PDF content will be:
1. Extracted as text
2. Split into chunks
3. Converted to embeddings using Azure OpenAI
4. Stored in the in-memory vector database

**Note**: The vector database is in-memory, so documents are re-processed each time the application starts.

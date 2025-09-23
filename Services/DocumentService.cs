using Advanced_RAG_Workshop.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace Advanced_RAG_Workshop.Services;

public class DocumentService
{
    private const int ChunkSize = 1000; // Characters per chunk
    private const int ChunkOverlap = 200; // Overlap between chunks
    private SemanticChunkingService _semanticChunkingService;

    public DocumentService(SemanticChunkingService semanticChunkingService)
    {
        _semanticChunkingService = semanticChunkingService;
    }

    public async Task<List<DocumentChunk>> ProcessDocumentsAsync(string documentsPath)
    {
        var chunks = new List<DocumentChunk>();
        
        if (!Directory.Exists(documentsPath))
        {
            Console.WriteLine($"Documents directory not found: {documentsPath}");
            return chunks;
        }

        var pdfFiles = Directory.GetFiles(documentsPath, "*.pdf", SearchOption.AllDirectories);
        
        if (pdfFiles.Length == 0)
        {
            Console.WriteLine($"No PDF files found in: {documentsPath}");
            return chunks;
        }

        Console.WriteLine($"📄 Processing {pdfFiles.Length} PDF file(s)...");

        foreach (var pdfFile in pdfFiles)
        {
            try
            {
                var fileChunks = await ProcessPdfAsync(pdfFile);
                chunks.AddRange(fileChunks);
                Console.WriteLine($"✅ Processed: {Path.GetFileName(pdfFile)} ({fileChunks.Count} chunks)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error processing {Path.GetFileName(pdfFile)}: {ex.Message}");
            }
        }

        Console.WriteLine($"📚 Total chunks created: {chunks.Count}");
        return chunks;
    }

    private Task<List<DocumentChunk>> ProcessPdfAsync(string pdfPath)
    {
        var chunks = new List<DocumentChunk>();

        var fullText = ExtractTextFromPdf(pdfPath);

        var textChunks = SplitTextIntoChunks(fullText, ChunkSize, ChunkOverlap);
        var fileName = Path.GetFileName(pdfPath);

        for (int i = 0; i < textChunks.Count; i++)
        {
            chunks.Add(new DocumentChunk
            {
                Id = $"{fileName}_chunk_{i}",
                Content = textChunks[i],
                SourceFile = fileName,
            });
        }

        return Task.FromResult(chunks);
    }

    private string ExtractTextFromPdf(string pdfPath)
    {
        using var pdfReader = new PdfReader(pdfPath);
        using var pdfDocument = new PdfDocument(pdfReader);

        var text = new System.Text.StringBuilder();
        
        for (int pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
        {
            var page = pdfDocument.GetPage(pageNum);
            var strategy = new SimpleTextExtractionStrategy();
            var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
            text.AppendLine(pageText);
        }
        
        return text.ToString();
    }

    private List<string> SplitTextIntoChunks(string text, int chunkSize, int overlap)
    {
        var chunks = new List<string>();
        
        if (string.IsNullOrWhiteSpace(text))
            return chunks;

        // Simple text splitting by character count with overlap
        for (int i = 0; i < text.Length; i += chunkSize - overlap)
        {
            int endIndex = Math.Min(i + chunkSize, text.Length);
            var chunk = text.Substring(i, endIndex - i).Trim();
            
            if (!string.IsNullOrWhiteSpace(chunk))
            {
                chunks.Add(chunk);
            }

            if (endIndex == text.Length)
                break;
        }

        return chunks;
    }

    public async Task<List<DocumentChunk>> ProcessDocumentsSemanticallyAsync(string documentsPath)
    {
        var chunks = new List<DocumentChunk>();

        if (!Directory.Exists(documentsPath))
        {
            Console.WriteLine($"Documents directory not found: {documentsPath}");
            return chunks;
        }

        var pdfFiles = Directory.GetFiles(documentsPath, "*.pdf", SearchOption.AllDirectories);

        if (pdfFiles.Length == 0)
        {
            Console.WriteLine($"No PDF files found in: {documentsPath}");
            return chunks;
        }

        Console.WriteLine($"📄 Processing {pdfFiles.Length} PDF file(s) with semantic chunking...");

        foreach (var pdfFile in pdfFiles)
        {
            try
            {
                var fileName = Path.GetFileName(pdfFile);
                var text = ExtractTextFromPdf(pdfFile);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    Console.WriteLine($"🧠 Creating semantic chunks for: {fileName}");
                    var fileChunks = await _semanticChunkingService.ChunkTextAsync(text, fileName);
                    chunks.AddRange(fileChunks);
                    Console.WriteLine($"✅ Created {fileChunks.Count} semantic chunks from: {fileName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error processing {Path.GetFileName(pdfFile)}: {ex.Message}");
            }
        }

        Console.WriteLine($"🎯 Total semantic chunks created: {chunks.Count}");
        return chunks;
    }

    public Task<List<(string FileName, string Content)>> ExtractTextFromDocumentsAsync(string documentsPath)
    {
        var documents = new List<(string FileName, string Content)>();

        if (!Directory.Exists(documentsPath))
        {
            Console.WriteLine($"Documents directory not found: {documentsPath}");
            return Task.FromResult(documents);
        }

        var pdfFiles = Directory.GetFiles(documentsPath, "*.pdf", SearchOption.AllDirectories);

        if (pdfFiles.Length == 0)
        {
            Console.WriteLine($"No PDF files found in: {documentsPath}");
            return Task.FromResult(documents);
        }

        Console.WriteLine($"📄 Extracting text from {pdfFiles.Length} PDF file(s)...");

        foreach (var pdfFile in pdfFiles)
        {
            var text = ExtractTextFromPdf(pdfFile);
            if (!string.IsNullOrWhiteSpace(text))
            {
                var fileName = Path.GetFileName(pdfFile);
                documents.Add((fileName, text));
                Console.WriteLine($"✅ Extracted text from: {fileName}");
            }
        }

        return Task.FromResult(documents);
    }
}

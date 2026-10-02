using System.Collections.ObjectModel;
namespace InDesignBookStudio.Models;
public class BookDocument {
 public string Title { get; set; } = "Untitled Book";
 public string Author { get; set; } = "";
 public double PageWidth { get; set; } = 595;
 public double PageHeight { get; set; } = 842;
 public ObservableCollection<BookPage> Pages { get; set; } = new();
}
public class BookPage {
 public string Name { get; set; } = "Page";
 public string Content { get; set; } = "";
 public double FontSize { get; set; } = 16;
 public string FontFamily { get; set; } = "Georgia";
 public bool IsBold { get; set; }
 public bool IsItalic { get; set; }
 public string TextAlignment { get; set; } = "Left";
}

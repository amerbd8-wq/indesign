using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using InDesignBookStudio.Models;

namespace InDesignBookStudio;

public partial class MainWindow : Window, INotifyPropertyChanged
{
 public BookDocument Book { get; private set; } = new();
 public IEnumerable<BookPage> Pages => Book.Pages;
 public IEnumerable<string> Fonts { get; } = new[]{"Georgia","Arial","Calibri","Times New Roman","Verdana","Segoe UI","Garamond"};
 public IEnumerable<double> FontSizes { get; } = new double[]{10,11,12,14,16,18,20,24,28,32,36,48,60,72};
 private BookPage? _selectedPage; private double _zoom=100; private string _currentFont="Georgia"; private double _currentFontSize=16; private bool _bold; private bool _italic; private string _statusText="Ready"; private string? _filePath;
 public BookPage? SelectedPage { get=>_selectedPage; set{_selectedPage=value;LoadPage();OnChanged(nameof(SelectedPage));OnChanged(nameof(PageInfo));} }
 public double PageWidth=>Book.PageWidth; public double PageHeight=>Book.PageHeight; public string PageSizeText=>$"{Book.PageWidth:0} × {Book.PageHeight:0} pt";
 public double Zoom { get=>_zoom; set{_zoom=value;OnChanged(nameof(Zoom));OnChanged(nameof(ZoomFactor));} } public double ZoomFactor=>Zoom/100;
 public string CurrentFont { get=>_currentFont; set{_currentFont=value;ApplyTypingFormat();OnChanged(nameof(CurrentFont));} }
 public double CurrentFontSize { get=>_currentFontSize; set{_currentFontSize=value;ApplyTypingFormat();OnChanged(nameof(CurrentFontSize));} }
 public FontWeight CurrentFontWeight=>_bold?FontWeights.Bold:FontWeights.Normal; public FontStyle CurrentFontStyle=>_italic?FontStyles.Italic:FontStyles.Normal;
 public string StatusText { get=>_statusText; set{_statusText=value;OnChanged(nameof(StatusText));} }
 public string PageInfo=>SelectedPage==null?"No page":$"Page {Book.Pages.IndexOf(SelectedPage)+1} of {Book.Pages.Count}";
 public ICommand NewPageCommand{get;} public ICommand DeletePageCommand{get;} public ICommand SaveCommand{get;} public ICommand SaveAsCommand{get;} public ICommand OpenCommand{get;} public ICommand BoldCommand{get;} public ICommand ItalicCommand{get;} public ICommand AlignLeftCommand{get;} public ICommand AlignCenterCommand{get;} public ICommand AlignRightCommand{get;}
 public MainWindow(){
  InitializeComponent();DataContext=this;
  NewPageCommand=new RelayCommand(_=>AddPage()); DeletePageCommand=new RelayCommand(_=>DeletePage(),_=>SelectedPage!=null&&Book.Pages.Count>1);
  SaveCommand=new RelayCommand(_=>Save(false));SaveAsCommand=new RelayCommand(_=>Save(true));OpenCommand=new RelayCommand(_=>Open());
  BoldCommand=new RelayCommand(_=>{_bold=!_bold;OnChanged(nameof(CurrentFontWeight));ApplyTypingFormat();}); ItalicCommand=new RelayCommand(_=>{_italic=!_italic;OnChanged(nameof(CurrentFontStyle));ApplyTypingFormat();});
  AlignLeftCommand=new RelayCommand(_=>SetAlignment(TextAlignment.Left));AlignCenterCommand=new RelayCommand(_=>SetAlignment(TextAlignment.Center));AlignRightCommand=new RelayCommand(_=>SetAlignment(TextAlignment.Right));
  AddPage();StatusText="Ready — create your book.";
 }
 void AddPage(){SaveCurrentPage();var p=new BookPage{Name=$"Page {Book.Pages.Count+1}"};Book.Pages.Add(p);SelectedPage=p;OnChanged(nameof(Pages));OnChanged(nameof(PageInfo));StatusText="New page created.";}
 void DeletePage(){if(SelectedPage==null||Book.Pages.Count<=1)return;var i=Book.Pages.IndexOf(SelectedPage);Book.Pages.Remove(SelectedPage);SelectedPage=Book.Pages[Math.Max(0,i-1)];OnChanged(nameof(Pages));OnChanged(nameof(PageInfo));}
 void LoadPage(){if(SelectedPage==null||Editor==null)return;Editor.Document.Blocks.Clear();var p=new Paragraph(new Run(SelectedPage.Content??""));p.FontFamily=new FontFamily(SelectedPage.FontFamily);p.FontSize=SelectedPage.FontSize;p.FontWeight=SelectedPage.IsBold?FontWeights.Bold:FontWeights.Normal;p.FontStyle=SelectedPage.IsItalic?FontStyles.Italic:FontStyles.Normal;p.TextAlignment=ParseAlignment(SelectedPage.TextAlignment);Editor.Document.Blocks.Add(p);_currentFont=SelectedPage.FontFamily;_currentFontSize=SelectedPage.FontSize;_bold=SelectedPage.IsBold;_italic=SelectedPage.IsItalic;OnChanged(nameof(CurrentFont));OnChanged(nameof(CurrentFontSize));OnChanged(nameof(CurrentFontWeight));OnChanged(nameof(CurrentFontStyle));}
 void SaveCurrentPage(){if(SelectedPage==null||Editor==null)return;var r=new TextRange(Editor.Document.ContentStart,Editor.Document.ContentEnd);SelectedPage.Content=r.Text.TrimEnd('\r','\n');SelectedPage.FontFamily=CurrentFont;SelectedPage.FontSize=CurrentFontSize;SelectedPage.IsBold=_bold;SelectedPage.IsItalic=_italic;SelectedPage.TextAlignment=Editor.Document.Blocks.FirstBlock is Paragraph p?p.TextAlignment.ToString():"Left";}
 void Editor_TextChanged(object sender,TextChangedEventArgs e){if(SelectedPage==null)return;SaveCurrentPage();StatusText="Editing…";}
 void ApplyTypingFormat(){if(Editor==null)return;Editor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty,new FontFamily(CurrentFont));Editor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty,CurrentFontSize);Editor.Selection.ApplyPropertyValue(TextElement.FontWeightProperty,CurrentFontWeight);Editor.Selection.ApplyPropertyValue(TextElement.FontStyleProperty,CurrentFontStyle);SaveCurrentPage();}
 void SetAlignment(TextAlignment a){if(Editor.Document.Blocks.FirstBlock is Paragraph p)p.TextAlignment=a;SaveCurrentPage();}
 void Save(bool choose){SaveCurrentPage();var path=_filePath;if(choose||string.IsNullOrWhiteSpace(path)){var d=new Microsoft.Win32.SaveFileDialog{Filter="InDesign Book (*.idbook)|*.idbook|JSON (*.json)|*.json",FileName="MyBook.idbook"};if(d.ShowDialog()!=true)return;path=d.FileName;_filePath=path;}File.WriteAllText(path!,JsonSerializer.Serialize(Book,new JsonSerializerOptions{WriteIndented=true}));StatusText=$"Saved: {Path.GetFileName(path)}";}
 void Open(){var d=new Microsoft.Win32.OpenFileDialog{Filter="InDesign Book (*.idbook;*.json)|*.idbook;*.json|All files (*.*)|*.*"};if(d.ShowDialog()!=true)return;var loaded=JsonSerializer.Deserialize<BookDocument>(File.ReadAllText(d.FileName));if(loaded==null)return;Book=loaded;if(Book.Pages.Count==0)Book.Pages.Add(new BookPage{Name="Page 1"});_filePath=d.FileName;OnChanged(nameof(Book));OnChanged(nameof(Pages));OnChanged(nameof(PageWidth));OnChanged(nameof(PageHeight));OnChanged(nameof(PageSizeText));SelectedPage=Book.Pages[0];StatusText=$"Opened: {Path.GetFileName(_filePath)}";}
 static TextAlignment ParseAlignment(string value)=>Enum.TryParse<TextAlignment>(value,out var a)?a:TextAlignment.Left;
 void Exit_Click(object sender,RoutedEventArgs e)=>Close(); public event PropertyChangedEventHandler? PropertyChanged; void OnChanged(string n)=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(n));
 sealed class RelayCommand:ICommand{readonly Action<object?> e;readonly Predicate<object?>? c;public RelayCommand(Action<object?> e,Predicate<object?>? c=null){this.e=e;this.c=c;}public bool CanExecute(object? p)=>c?.Invoke(p)??true;public void Execute(object? p)=>e(p);public event EventHandler? CanExecuteChanged;}
}

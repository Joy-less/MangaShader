using System.Reflection;

public partial class Main : Node {
    [Export] public Button InputFilesButton { get; set; }
    [Export] public Button OutputDirectoryButton { get; set; }
    [Export] public OptionButton OutputFormatButton { get; set; }
    [Export] public BaseButton GenerateButton { get; set; }
    [Export] public FileDialog InputFileDialog { get; set; }
    [Export] public FileDialog OutputFileDialog { get; set; }
    [Export] public SubViewport RenderSubViewport { get; set; }
    [Export] public TextureRect RenderTextureRect { get; set; }

    private static readonly string[] ImageExtensions = ["png", "jpeg", "jpg", "webp", "svg", "bmp", "dds", "ktx", "exr", "hdr", "tga", /*"jxl", "avif"*/];

    public override void _Ready() {
        SetupFileDialogButton(InputFilesButton, InputFileDialog, RenderPreview);
        SetupFileDialogButton(OutputDirectoryButton, OutputFileDialog);

        SetOptionButtonItems<ImageFormatType>(OutputFormatButton, Format => Format.ToString().ToSnakeCase().Capitalize());

        string ImageExtensionsFilter = string.Join(",", ImageExtensions.Select(ImageExtension => $"*.{ImageExtension}"));
        InputFileDialog.AddFilter(ImageExtensionsFilter, "Image Files");
        OutputFileDialog.AddFilter(ImageExtensionsFilter, "Image Files");

        GenerateButton.Pressed += () => _ = GenerateAsync();
    }

    private static void SetupFileDialogButton(Button Button, FileDialog FileDialog, Action? OnChange = null) {
        Button.Pressed += () => {
            FileDialog.Popup();
        };
        FileDialog.FileSelected += (string Path) => {
            Button.Text = Path;
            Button.TooltipText = Button.Text;
            OnChange?.Invoke();
        };
        FileDialog.DirSelected += (string Path) => {
            Button.Text = Path;
            Button.TooltipText = Button.Text;
            OnChange?.Invoke();
        };
        FileDialog.FilesSelected += (string[] Paths) => {
            Button.Text = string.Join(";", Paths);
            Button.TooltipText = Button.Text;
            OnChange?.Invoke();
        };
    }
    private void RenderPreview() {
        SetImage(GetImagePaths(InputFilesButton.Text).First());
    }
    private async GDTask GenerateAsync() {
        GD.Print(string.Join(", ", GetImagePaths(InputFilesButton.Text)));
        foreach (string ImagePath in GetImagePaths(InputFilesButton.Text)) {
            Image RenderedImage = await RenderImageAsync(ImagePath);
            SaveImage(RenderedImage, ImagePath, OutputDirectoryButton.Text, (ImageFormatType)OutputFormatButton.GetSelectedId());
        }
        RenderPreview();
    }

    private void SetImage(string? ImagePath) {
        if (ImagePath is not null) {
            Image InputImage = Image.LoadFromFile(ImagePath);
            ImageTexture InputTexture = ImageTexture.CreateFromImage(InputImage);
            Vector2I InputImageSize = InputImage.GetSize();

            RenderTextureRect.Texture = InputTexture;
            RenderSubViewport.Size = InputImageSize;
            RenderTextureRect.Size = InputImageSize;
        }
        else {
            RenderTextureRect.Texture = null;
        }
    }
    private async GDTask<Image> RenderImageAsync(string ImagePath) {
        SetImage(ImagePath);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        Image RenderedImage = RenderSubViewport.GetTexture().GetImage();
        SetImage(null);
        return RenderedImage;
    }
    private string[] GetImagePaths(string Path) {
        Path = Path.Trim();

        // Multiple
        if (Path.Contains(';')) {
            return [.. Path.Split(';', StringSplitOptions.TrimEntries).SelectMany(GetImagePaths)];
        }

        // File
        if (FileAccess.FileExists(Path)) {
            return [Path];
        }
        // Directory
        else if (DirAccess.DirExistsAbsolute(Path)) {
            IEnumerable<string> Files = DirAccess.GetFilesAt(Path);
            Files = Files.Where(File => ImageExtensions.Any(ImageExtension => File.GetExtension().Equals(ImageExtension, StringComparison.OrdinalIgnoreCase)));
            Files = Files.Select(File => Path.PathJoin(File));
            return [.. Files];
        }
        // None
        else {
            return [];
        }
    }
    private Error SaveImage(Image Image, string ImagePath, string OutputDirectory, ImageFormatType FormatType) {
        string SavePath = OutputDirectory.PathJoin(ImagePath.GetFile());

        /*switch (FormatType) {
            case ImageFormatType.Png: {
                SavePath = System.IO.Path.ChangeExtension(SavePath, "png");
                return Image.SavePng(SavePath);
            }
            case ImageFormatType.Jpeg: {
                SavePath = System.IO.Path.ChangeExtension(SavePath, "jpeg");
                return Image.SaveJpg(SavePath);
            }
            case ImageFormatType.LosslessWebp: {
                SavePath = System.IO.Path.ChangeExtension(SavePath, "webp");
                return Image.SaveWebp(SavePath);
            }
            case ImageFormatType.LossyWebp: {
                SavePath = System.IO.Path.ChangeExtension(SavePath, "webp");
                return Image.SaveWebp(SavePath, lossy: true);
            }
            default: {
                throw new NotImplementedException(FormatType.ToString());
            }
        }*/

        SavePath = System.IO.Path.ChangeExtension(SavePath, FormatType switch {
            ImageFormatType.Png => "png",
            ImageFormatType.Jpeg => "jpeg",
            ImageFormatType.LosslessWebp => "webp",
            ImageFormatType.LossyWebp => "webp",
            _ => throw new NotImplementedException(FormatType.ToString())
        });
        SavePath = MakeFilePathUnique(SavePath);
        GD.Print(SavePath);

        return FormatType switch {
            ImageFormatType.Png => Image.SavePng(SavePath),
            ImageFormatType.Jpeg => Image.SaveJpg(SavePath),
            ImageFormatType.LosslessWebp => Image.SaveWebp(SavePath),
            ImageFormatType.LossyWebp => Image.SaveWebp(SavePath, lossy: true),
            _ => throw new NotImplementedException(FormatType.ToString())
        };
    }
    private string MakeFilePathUnique(string Path) {
        string PathBaseName = Path.GetBaseName();
        string PathExtension = Path.GetExtension();
        long Counter = 0;
        while (FileAccess.FileExists(Path)) {
            Path = PathBaseName + $"_{Counter}" + "." + PathExtension;
            Counter++;
        }
        return Path;
    }
    private void SetOptionButtonItems<T>(OptionButton OptionButton, Func<T, string> GetDisplayName) where T : struct, Enum {
        OptionButton.Clear();
        foreach (T Item in Enum.GetValues<T>()) {
            OptionButton.AddItem(GetDisplayName(Item), (int)(object)Item);
        }
    }
}

public enum ImageFormatType {
    Png,
    Jpeg,
    LosslessWebp,
    LossyWebp,
}
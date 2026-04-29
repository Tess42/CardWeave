using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace Tablet_weaving_pattern_designer
{
    /// <summary>
    /// Provides file handling utilities for working with <see cref="TabletBand"/> patterns.
    /// Supports saving and loading user‑created patterns, retrieving preset files, and exporting rendered pattern bitmaps.
    /// </summary>
    public static class BandFileService
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = true };

        /// <summary>
        /// Opens a save dialog and stores the given <see cref="TabletBand"/> as a JSON file.
        /// Returns the selected file path, or null if the user cancels the dialog.
        /// </summary>
        public static async Task<string?> SaveBandAsAsync(TabletBand band)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                Title = "Save pattern to",
                DefaultExt = ".json",
                FileName = "unnamed pattern.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string json = JsonSerializer.Serialize(band, JsonOptions);
                await File.WriteAllTextAsync(saveFileDialog.FileName, json);

                return saveFileDialog.FileName;
            }

            return null;
        }

        /// <summary>
        /// Saves the given <see cref="TabletBand"/> to the specified path if provided.
        /// If no path exists, opens a save dialog and stores the band as a JSON file.
        /// Returns the file path, or null if saving fails or the dialog is cancelled.
        /// </summary>
        public static async Task<string?> SaveBandAsync(TabletBand band, string? existingPath)
        {
            if (!string.IsNullOrEmpty(existingPath))
            {
                string json = JsonSerializer.Serialize(band, JsonOptions);
                await File.WriteAllTextAsync(existingPath, json);

                return existingPath;
            }

            return await SaveBandAsAsync(band);
        }

        /// <summary>
        /// Opens a file dialog and loads a user‑saved pattern from a JSON file.
        /// Returns the deserialized <see cref="TabletBand"/> instance, or null if the user cancels the dialog.
        /// Throws exceptions if the file cannot be read or the JSON cannot be deserialized.
        /// </summary>
        public static async Task<TabletBand?> LoadBandAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                Title = "Open saved pattern",
                DefaultExt = ".json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string json = await File.ReadAllTextAsync(openFileDialog.FileName);
                var band = JsonSerializer.Deserialize<TabletBand>(json, JsonOptions) 
                    ?? throw new Exception("The selected file does not contain a valid pattern.");

                band.CurrentFilePath = openFileDialog.FileName;
                return band;
            }

            return null;
        }

        /// <summary>
        /// Returns a list of all preset pattern files located in the application's Patterns folder.
        /// Returns an empty list if the folder does not exist.
        /// </summary>
        public static List<string> GetPresetFiles()
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Patterns");

            if (!Directory.Exists(folder))
            {
                return new List<string>();
            }

            return Directory.GetFiles(folder, "*.json").ToList();
        }

        /// <summary>
        /// Renders the given grid into a bitmap and saves it as an image file.
        /// Supports PNG and JPEG formats depending on file extension.
        /// </summary>
        public static async Task ExportPatternAsImageAsync(Grid target, string filePath)
        {
            target.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            target.Arrange(new Rect(target.DesiredSize));

            var bitmap = new RenderTargetBitmap(
                (int)target.ActualWidth,
                (int)target.ActualHeight - 15,
                96, 96,
                PixelFormats.Pbgra32);

            bitmap.Render(target);

            await SaveBitmapAsync(bitmap, filePath);
        }


        /// <summary>
        /// Saves the specified pattern bitmap to the given file path using PNG or JPEG encoding.
        /// Uses JPEG quality level 95 when saving .jpg files.
        /// </summary>
        public static async Task SaveBitmapAsync(RenderTargetBitmap bitmap, string filePath)
        {
            BitmapEncoder encoder = filePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) 
                ? new JpegBitmapEncoder { QualityLevel = 95 } 
                : new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var fs = File.Create(filePath))
            {
                encoder.Save(fs);
            }

            await Task.CompletedTask;
        }
    }
}

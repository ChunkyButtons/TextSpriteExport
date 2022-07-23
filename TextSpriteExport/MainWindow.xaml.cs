using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace TextSpriteExport
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window{

		const int DefaultFontSize = 16;
		FontFamily CurrentFontFamily;
		
		double fontWidth = 0d;
		double fontHeightMax = 0d;
		int columnCount = 0;		
		int userCharMin = 0;
		int userCharMax = 128;
		bool useAscii = false;
		//List<Rectangle> rects = new List<Rectangle>();	

		public MainWindow(){
			InitializeComponent();
			
			CBFontFamily.SelectedIndex = 0; //this will call the event to init font family
			
			if(useAscii == true){
				GenerateAsciiText();
			}else{
				GenerateCharText();
			}

			TBFontSize.Text = DefaultFontSize.ToString();

			//MessageBox.Show(Directory.GetCurrentDirectory() + @"\Fonts\");

			foreach (FontFamily curFF in Fonts.GetFontFamilies(Directory.GetCurrentDirectory() + @"\Fonts\")){
				CBFontFamily.Items.Add(curFF.ToString());
			}

			CBTextFormat.Items.Add("Display");
			CBTextFormat.Items.Add("Ideal");
			CBTextFormat.SelectedIndex = 0;

			CBTextRendering.Items.Add("Aliased");
			CBTextRendering.Items.Add("Grayscale");
			CBTextRendering.Items.Add("ClearType");
			CBTextRendering.SelectedIndex = 0;

			CBTextHinting.Items.Add("Fixed");
			CBTextHinting.Items.Add("Animated");
			CBTextHinting.SelectedIndex = 0;

			TextBlockTest.SnapsToDevicePixels = true;

			UpdateRenderGrid();
		}
		
		private void UpdateRenderGrid(){
			MainWindowElement.SizeToContent = SizeToContent.WidthAndHeight;

			if (SVRender != null && BorderTest != null) {
				BorderTest.Width = SVRender.Width;
			}
	
			//Setting scrollviewer to main window width minus padding does not work
			//if(SVRender != null){
			//	SVRender.Width = MainWindowElement.Width-10; //10px padding
			//}

			if (useAscii == true) {
				GenerateAsciiText();
			} else {
				GenerateCharText();
			}

			if (!IsMonospaced(CurrentFontFamily)) {
				if(CBFontFamily.SelectedItem != null){
					MessageBox.Show($"{CBFontFamily.SelectedItem.ToString()}\n\nWarning! It appears this font is NOT monospaced.");
				}
			}
			
			if (SpriteGrid == null){
				return;
			}
			
			//RenderCanvas.Width = TextBlockTest.RenderSize.Width;
			//RenderCanvas.Height = TextBlockTest.RenderSize.Height;

			SpriteGrid.Children.Clear();

			if (CheckBoxGrid.IsChecked.Value == false) {
				return; //don't bother rendering grid shapes if not enabled
			}

			if (CheckBoxGrid.IsChecked == true){
				SolidColorBrush fillBrush = new SolidColorBrush();
				fillBrush.Color = Colors.AliceBlue;
				fillBrush.Opacity = 0.0d;

				SolidColorBrush strokeBrush = new SolidColorBrush();
				strokeBrush.Color = Colors.Black;
				strokeBrush.Opacity = 0.5d;
				
				int itemCount = 0;
				int curCol = 0;

				for (int i2 = 0; i2 < userCharMax-userCharMin; i2++){
					Rectangle tempRect = new Rectangle();
					tempRect.Stroke = strokeBrush;
					tempRect.Fill = fillBrush;
					tempRect.HorizontalAlignment = HorizontalAlignment.Left;
					tempRect.VerticalAlignment = VerticalAlignment.Top;
					tempRect.Height = fontHeightMax;
					tempRect.Width = fontWidth;
					SpriteGrid.Children.Add(tempRect);

					Canvas.SetLeft(tempRect, itemCount * fontWidth);
					Canvas.SetTop(tempRect, curCol * fontHeightMax);
					itemCount++;

					if(itemCount == columnCount){
						curCol++;
						itemCount = 0;
					}
				}
			}
		}
		
		private void SaveImage(){
			//DrawingVisual is the only way (I could find) to render a UIElement with all its children, otherwise you can only call RenderTargetBitmap for one element at a time 

			//RenderCanvas.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			//RenderCanvas.Arrange(new Rect(0, 0, 9999, 9999));
			RenderCanvas.UpdateLayout();

			DrawingVisual drawingVisual = new DrawingVisual();	

			using(DrawingContext context = drawingVisual.RenderOpen()){
				VisualBrush brush = new VisualBrush(RenderCanvas) { Stretch = Stretch.None, AutoLayoutContent = false };
				brush.TileMode = TileMode.None;
				context.DrawRectangle(brush, null, new Rect(0, 0, RenderCanvas.ActualWidth, RenderCanvas.ActualHeight));
				context.Close();
			}
			
			RenderTargetBitmap rtb = new RenderTargetBitmap(
				(int)RenderCanvas.ActualWidth,
				(int)RenderCanvas.ActualHeight,
				96d,
				96d,
				System.Windows.Media.PixelFormats.Default
			);

			rtb.Render(drawingVisual);
			
			//CroppedBitmap crop = new CroppedBitmap(rtb, new Int32Rect(0, 0, 20, 75));

			BitmapEncoder pngEncoder = new PngBitmapEncoder();

			pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
			//pngEncoder.Frames.Add(BitmapFrame.Create(crop));
			
			System.Windows.Forms.OpenFileDialog SaveImageDialog = new System.Windows.Forms.OpenFileDialog();
			SaveImageDialog.CheckFileExists = false;
			SaveImageDialog.CheckPathExists = true;
			SaveImageDialog.Multiselect = false;			
			SaveImageDialog.Filter = "Portable Network Graphic (.png)|*.png;|All Files (*.*)|*.*";

			string exportFontName = CBFontFamily.SelectedItem.ToString(); 
			
			if(exportFontName.Contains("#")){
				exportFontName = exportFontName.Split('#')[1];
			}
			
			exportFontName = System.Text.RegularExpressions.Regex.Replace(exportFontName, @"[\/?:*""><|]+", "", System.Text.RegularExpressions.RegexOptions.Compiled);

			SaveImageDialog.FileName = $"{exportFontName}_Size{TBFontSize.Text}_{fontWidth}x{fontHeightMax}";

			if(SaveImageDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK){
				using (var fs = System.IO.File.OpenWrite(SaveImageDialog.FileName)) {
					pngEncoder.Save(fs);
				}
			}
		}

		private void GenerateAsciiText(){
			if (TextBlockTest == null) {
				return;
			}

			byte[] asciiArray = new byte[256];

			for (int i = userCharMin; i < userCharMax; i++){
				try{
				asciiArray[i] = (byte)i;
				}catch{
					MessageBox.Show(i.ToString());
				}
			}

			string stringRequest = Encoding.ASCII.GetString(asciiArray).Trim();

			if (columnCount > 0) {
				StringBuilder finalOutputSB = new StringBuilder();

				int curCnt = 0;
				for (int coli = userCharMin; coli < stringRequest.Length; coli++) {
					finalOutputSB.Append(stringRequest[coli]);
					curCnt++;

					if (curCnt == columnCount && coli != stringRequest.Length - 1) {
						//Add newline for each column break, except when at the end of the string
						finalOutputSB.Append("\n");
						curCnt = 0;
					}
				}

				TextBlockTest.Content = finalOutputSB.ToString();
			} else {
				TextBlockTest.Content = stringRequest;
			}
		}

		private void GenerateCharText(){
			if(TextBlockTest == null){
				return;
			}
			
			if(userCharMax > 128){
				userCharMax = 128;
				TBCharMax.Text = "128";
			}

			char[] charArray = new char[128];
			
			for (int i = userCharMin; i < userCharMax; i++){
				char tempChar =  (char)i;

				if(char.IsWhiteSpace(tempChar) || char.IsControl(tempChar)){
					continue;
				}
				
				charArray[i] = tempChar;
			}
			
			string stringRequest = new string(charArray);

			if (columnCount > 0){
				StringBuilder finalOutputSB = new StringBuilder();

				int curCnt = 0;
				for(int coli=userCharMin; coli< stringRequest.Length; coli++){
					finalOutputSB.Append(stringRequest[coli]);
					curCnt++;
				
					if(curCnt == columnCount && coli != stringRequest.Length-1) {
						//Add newline for each column break, except when at the end of the string
						finalOutputSB.Append("\n");
						curCnt = 0;
					}
				}

				TextBlockTest.Content = finalOutputSB.ToString();
			} else{
				TextBlockTest.Content = stringRequest;
			}
		}
		private void CBFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e){
			//ComboBox cbFontFamilyTemp = (ComboBox)sender;

			CurrentFontFamily = new FontFamily(CBFontFamily.SelectedItem.ToString());
			TextBlockTest.FontFamily = CurrentFontFamily;
			
			//MessageBox.Show($"Width: {fontWidth} Height: {fontHeightMax}");
			UpdateRenderGrid();
		}

		private void CheckBoxBold_Checked(object sender, RoutedEventArgs e){
			TextBlockTest.FontWeight = FontWeights.Bold;
		}

		private void CheckBoxBold_Unchecked(object sender, RoutedEventArgs e){
			TextBlockTest.FontWeight = FontWeights.Normal; //Regular and Normal are the same
		}

		private void CheckBoxItalic_Checked(object sender, RoutedEventArgs e){
			TextBlockTest.FontStyle = FontStyles.Italic; //Regular and Normal are the same
		}

		private void CheckBoxItalic_Unchecked(object sender, RoutedEventArgs e){
			TextBlockTest.FontStyle = FontStyles.Normal; //Regular and Normal are the same
		}

		private void TBFontSize_TextChanged(object sender, TextChangedEventArgs e){
			if(String.IsNullOrEmpty(TBFontSize.Text)){
				return;
			}
			
			int intOutput = 0;

			string changeAttempt = TBFontSize.Text.Substring(TBFontSize.Text.Length-1, 1);
			
			if(changeAttempt == "."){
				//User attempting to add decimal point
				if(TBFontSize.Text.IndexOf(".") < TBFontSize.Text.Length-1){
					//Already contains a decimal point, don't allow another
					TBFontSize.Text = TBFontSize.Text.Substring(0, TBFontSize.Text.Length-1);
				}else{
					//Valid addition of decimal point
					return; //don't do any additional checks to allow user to enter remaining digits
				}
			}else{
				//User adding non-decimal point character
				if(int.TryParse(changeAttempt, out intOutput) == false){
					//user attempted to add non-integer, do not allow
					TBFontSize.Text = TBFontSize.Text.Substring(0, TBFontSize.Text.Length - 1);
				}
			}
			
			//Finally round to one decimal place, verify input is numeric (should only cause issue at initial load)

			double dOutput;

			if(double.TryParse(TBFontSize.Text, out dOutput) == false){
				TBFontSize.Text = DefaultFontSize.ToString();
			}
			
			dOutput = Math.Round(double.Parse(TBFontSize.Text), 2); //rount to two digits (if applicable)

			TBFontSize.Text = dOutput.ToString();

			TBFontSize.CaretIndex = TBFontSize.Text.Length;
			
			if(dOutput <= 0){
				dOutput = DefaultFontSize;
			}

			if(TextBlockTest != null){
				TextBlockTest.FontSize = dOutput;
			}
			UpdateRenderGrid();
		}

		private bool IsMonospaced(FontFamily family){
			if(family == null){
				return false;
			}
		
			fontHeightMax = 0; //reset this each time for new comparisons

			//From https://stackoverflow.com/questions/21965321/check-is-font-monospace lol
			char[] charSizes = new char[127];

			foreach (Typeface typeface in family.GetTypefaces()){
				bool widthFound = false;
				for(int chari=0; chari<charSizes.Length; chari++){
					charSizes[chari] = (char)chari;

					if (char.IsWhiteSpace(charSizes[chari]) == true){
						continue;
					}

					FormattedText formattedText = new FormattedText(
						charSizes[chari].ToString(),
						System.Globalization.CultureInfo.CurrentCulture,
						FlowDirection.LeftToRight,
						typeface,
						TextBlockTest.FontSize,
						Brushes.Black,
						new NumberSubstitution(),
						TextOptions.GetTextFormattingMode(TextBlockTest),
						1
					);
		
					if (widthFound == false && formattedText.Width > 0) {  // first char in list with a width
						fontWidth = Math.Round(formattedText.Width, 2);
						widthFound = true;
					}else{
						if (Math.Round(formattedText.Width, 2) != fontWidth && formattedText.Width > 0) {
							//MessageBox.Show($"{formattedText.Width} -- {fontWidth}");
							return false;
						}
					}
					if (formattedText.Height > fontHeightMax) {
						fontHeightMax = Math.Round(formattedText.Height, 2);
					}
				}
			}

			CharDimensions.Content = $"Character Width: {fontWidth} Character Height: {fontHeightMax}";
			
			//ExportBackground.Height = columnCount > 0 ? fontHeightMax * (TextBlockTest.Content.ToString().Length/2) : fontHeightMax;

			return true;
		}

		private void TBColumns_TextChanged(object sender, TextChangedEventArgs e){
			if (String.IsNullOrEmpty(TBColumns.Text)) {
				return;
			}

			int intOutput = 0;

			string changeAttempt = TBColumns.Text.Substring(TBColumns.Text.Length - 1, 1);

			if (int.TryParse(changeAttempt, out intOutput) == false) {
				//user attempted to add non-integer, do not allow
				TBColumns.Text = TBColumns.Text.Substring(0, TBColumns.Text.Length - 1);
			}

			if(int.TryParse(TBColumns.Text, out intOutput) == false){
				intOutput = 0;
			}

			TBColumns.Text = intOutput.ToString();

			TBColumns.CaretIndex = TBColumns.Text.Length;

			if (intOutput < 0) {
				intOutput = 0;
			}

			columnCount = intOutput;
			UpdateRenderGrid();
		}

		private void TBCharMin_TextChanged(object sender, TextChangedEventArgs e) {
			if (String.IsNullOrEmpty(TBCharMin.Text)) {
				return;
			}

			int intOutput = 0;

			string changeAttempt = TBCharMin.Text.Substring(TBCharMin.Text.Length - 1, 1);

			if (int.TryParse(changeAttempt, out intOutput) == false) {
				//user attempted to add non-integer, do not allow
				TBCharMin.Text = TBCharMin.Text.Substring(0, TBCharMin.Text.Length - 1);
			}

			if (int.TryParse(TBCharMin.Text, out intOutput) == false) {
				intOutput = 0;
			}

			if (intOutput > 128 && useAscii == false) {
				intOutput = 128;
			}

			if (intOutput > 256 && useAscii == true) {
				intOutput = 256;
			}

			if (intOutput <= 0) {
				intOutput = 0;
			}

			TBCharMin.Text = intOutput.ToString();

			TBCharMin.CaretIndex = TBCharMin.Text.Length;

			userCharMin = intOutput;

			UpdateRenderGrid();
		}

		private void TBCharMax_TextChanged(object sender, TextChangedEventArgs e) {
			if (String.IsNullOrEmpty(TBCharMax.Text)) {
				return;
			}

			int intOutput = 0;

			string changeAttempt = TBCharMax.Text.Substring(TBCharMax.Text.Length - 1, 1);

			if (int.TryParse(changeAttempt, out intOutput) == false) {
				//user attempted to add non-integer, do not allow
				TBCharMax.Text = TBCharMax.Text.Substring(0, TBCharMax.Text.Length - 1);
			}

			if (int.TryParse(TBCharMax.Text, out intOutput) == false) {
				intOutput = 0;
			}

			if (intOutput > 128 && useAscii == false) {
				intOutput = 128;
			}

			if (intOutput > 256 && useAscii == true) {
				intOutput = 256;
			}

			if (intOutput <= 0) {
				intOutput = 1;
			}

			TBCharMax.Text = intOutput.ToString();

			TBCharMax.CaretIndex = TBCharMax.Text.Length;

			userCharMax = intOutput;
			
			UpdateRenderGrid();
		}

		private void CheckBoxGrid_Click(object sender, RoutedEventArgs e) {
			UpdateRenderGrid();
		}

		private void CheckBoxSnap_Checked(object sender, RoutedEventArgs e) {
			if(RenderCanvas != null){
				RenderCanvas.SnapsToDevicePixels = true;
				foreach(UIElement curEl in RenderCanvas.Children){
					curEl.SnapsToDevicePixels = true;
				}
			}
		}

		private void CheckBoxSnap_Unchecked(object sender, RoutedEventArgs e) {
			if (RenderCanvas != null) {
				RenderCanvas.SnapsToDevicePixels = false;
				foreach (UIElement curEl in RenderCanvas.Children) {
					curEl.SnapsToDevicePixels = false;
				}
			}
		}

		private void CBTextFormat_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (CBTextFormat.SelectedItem.ToString() == "Display") {
				TextOptions.SetTextFormattingMode(TextBlockTest, TextFormattingMode.Display);
			}

			if (CBTextFormat.SelectedItem.ToString() == "Ideal") {
				TextOptions.SetTextFormattingMode(TextBlockTest, TextFormattingMode.Ideal);
			}

			UpdateRenderGrid();
		}

		private void BtnSave_Click(object sender, RoutedEventArgs e) {
			SaveImage();
		}

		private void CheckBoxAscii_Checked(object sender, RoutedEventArgs e) {
			useAscii = true;
			UpdateRenderGrid();
		}

		private void CheckBoxAscii_Unchecked(object sender, RoutedEventArgs e) {
			useAscii = false;
			UpdateRenderGrid();
		}

		private void TBCharMax_LostFocus(object sender, RoutedEventArgs e) {
			int intOutput = 128;
			int.TryParse(TBCharMax.Text, out intOutput);
			if (intOutput < userCharMin) {
				intOutput = userCharMin + 1;
				userCharMax = intOutput;
				TBCharMax.Text = intOutput.ToString();
			}
		}

		private void TBCharMin_LostFocus(object sender, RoutedEventArgs e) {
			int intOutput = 0;
			int.TryParse(TBCharMin.Text, out intOutput);

			if (intOutput > userCharMax) {
				intOutput = userCharMax - 1;
				userCharMin = intOutput;
				TBCharMin.Text = intOutput.ToString();
			}
		}

		private void TBFontSize_GotFocus(object sender, RoutedEventArgs e) {
			TBFontSize.SelectAll();
		}

		private void TBColumns_GotFocus(object sender, RoutedEventArgs e) {
			TBColumns.SelectAll();
		}

		private void TBCharMin_GotFocus(object sender, RoutedEventArgs e) {
			TBCharMin.SelectAll();
		}

		private void TBCharMax_GotFocus(object sender, RoutedEventArgs e) {
			TBCharMax.SelectAll();
		}

		private void CheckboxBackground_Unchecked(object sender, RoutedEventArgs e) {
			if (BtnColorPicker == null) {
				return;
			}
			StackPanelBkg.Visibility = Visibility.Collapsed;
			ExportBackground.Visibility = Visibility.Hidden;
		}

		private void CheckboxBackground_Checked(object sender, RoutedEventArgs e) {
			if(BtnColorPicker == null){
				return;
			}
			StackPanelBkg.Visibility = Visibility.Visible;
			ExportBackground.Visibility = Visibility.Visible;
		}

		private void BtnColorPicker_Clicked(object sender, RoutedEventArgs e) {
			System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
			colorDialog.SolidColorOnly = false;
			colorDialog.FullOpen = true;
			if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				ExportBackground.Fill = new SolidColorBrush(Color.FromArgb((byte)int.Parse(TBAlpha.Text), colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
			}
		}

		private void TBAlpha_GotFocus(object sender, RoutedEventArgs e) {
			TBAlpha.SelectAll();
		}

		private void TBAlpha_TextChanged(object sender, TextChangedEventArgs e) {
			if (String.IsNullOrEmpty(TBAlpha.Text)) {
				return;
			}

			int intOutput = 0;

			string changeAttempt = TBAlpha.Text.Substring(TBAlpha.Text.Length - 1, 1);

			if (int.TryParse(changeAttempt, out intOutput) == false) {
				//user attempted to add non-integer, do not allow
				TBAlpha.Text = TBAlpha.Text.Substring(0, TBAlpha.Text.Length - 1);
			}

			if (int.TryParse(TBAlpha.Text, out intOutput) == false) {
				intOutput = 255;
			}

			if (intOutput > 255) {
				intOutput = 255;
			}

			if (intOutput <= 0) {
				intOutput = 0;
			}

			TBAlpha.Text = intOutput.ToString();
			TBAlpha.CaretIndex = TBAlpha.Text.Length;

			if (ExportBackground == null){
				return;
			}
			
			SolidColorBrush oldBrush = (SolidColorBrush)ExportBackground.Fill;

			ExportBackground.Fill = new SolidColorBrush(Color.FromArgb((byte)intOutput, oldBrush.Color.R, oldBrush.Color.G, oldBrush.Color.B));
		}

		private void CBTextRendering_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (CBTextRendering.SelectedItem.ToString() == "Aliased") {
				TextOptions.SetTextRenderingMode(TextBlockTest, TextRenderingMode.Aliased);
			}

			if (CBTextRendering.SelectedItem.ToString() == "Grayscale") {
				TextOptions.SetTextRenderingMode(TextBlockTest, TextRenderingMode.Grayscale);
			}

			if (CBTextRendering.SelectedItem.ToString() == "ClearType") {
				TextOptions.SetTextRenderingMode(TextBlockTest, TextRenderingMode.ClearType);
			}

			UpdateRenderGrid();
		}

		private void CBTextHinting_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (CBTextHinting.SelectedItem.ToString() == "Fixed") {
				TextOptions.SetTextHintingMode(TextBlockTest, TextHintingMode.Fixed);
			}

			if (CBTextHinting.SelectedItem.ToString() == "Animated") {
				TextOptions.SetTextHintingMode(TextBlockTest, TextHintingMode.Animated);
			}


			UpdateRenderGrid();
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dataway.ImageLibrary;
using System.IO;

namespace TestImaging
{
	public partial class Form1 : Form
	{
		private Color applyColour = Color.Red;


		public string AssetDirectory
		{
			get
			{
				string dir = AppDomain.CurrentDomain.BaseDirectory + "Assets";
				return dir;

				//return @"C:\Users\Timothy\Dropbox\Sandbox\Libraries\Dataway.ImageLibrary\TestImaging\Assets";
			}
		}

		Bitmap processedImage;
		string filename = "";

		public Form1()
		{
			InitializeComponent();


			OpenFileDialog ofd = new OpenFileDialog();
			ofd.ShowDialog();

			//Image img = null;

			filename = ofd.FileName;
			processedImage = new Bitmap(ofd.FileName);


			////var image = new Bitmap(@"C:\Users\tim.HQ\Pictures\Growth_of_cubic_bacteria_25x16.jpg");
			//processedImage = ImageManipulator.GetScaledAspectImage(image, new Size(300, 300), false, true) as Bitmap;

			pictureBox1.Image = this.processedImage;

			//ImageManipulator.GreyScale(processedImage);
			////ImageManipulator.Invert(processedImage);

			//ImageManipulator.ApplyGreyScaleMask(processedImage, new Bitmap(@"C:\Projects\Wrightside\HummBeeHunt\HummbeeHunt.Website\HummbeeHunt.Website\Images\prizeMask.png"), false);

			////ImageManipulator.SetGamma(processedImage, 1, 2, 1);
			////ImageManipulator.SetBrightness(processedImage, -200);
			////ImageManipulator.SetContrast(processedImage, -50);
			////ImageManipulator.RotateFlip(processedImage, RotateFlipType.Rotate270FlipX);
			////ImageManipulator.Blur(processedImage, 20);


			ImageManipulator.ManipluatePixel(processedImage, (c) => { return new SafeColor(c.R, c.G / 2, 0); });


			//string blah = "blah";
			//UpdateCrop();

			comboBox1.Items.Clear();

			var blends = Enum.GetValues(typeof(BlendMode));
			foreach (var thing in blends)
			{
				comboBox1.Items.Add(thing.ToString());
			}
			//comboBox1.Items.AddRange(blends);
		}

		private void numX_ValueChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		private void numY_ValueChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		private void numWidth_ValueChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		private void numHeight_ValueChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		private void UpdatePreview()
		{
			var bmp = new Bitmap(filename);

			var hhh = bmp.Clone() as Image;

			//var outImage = ImageManipulator.GetCropped(hhh, new Rectangle((int)numX.Value, (int)numY.Value, (int)numWidth.Value, (int)numHeight.Value));

			var outImage = ImageManipulator.GetScaledAspectImage(hhh, new System.Drawing.Size(400, 400), false, false);


			//BlendMode blend = BlendMode.Alpha;
			//blend = (BlendMode)Enum.Parse(typeof(BlendMode), comboBox1.SelectedItem.ToString());
			//Image pattern = ImageManipulator.GetScaledAspectImage(new Bitmap(Path.Combine(this.AssetDirectory, "Pattern2.png")), new System.Drawing.Size(259, 259), false, false);
			//outImage = ImageManipulator.OverlayImage(outImage, pattern, blend, (int)numOpacity.Value);

			outImage = ImageManipulator.ColorImage(outImage, this.applyColour, (c1, c2) =>
			{
				Color s1 = Color.FromArgb(200, 255, 200); // Replace Red with this colour
				Color s2 = Color.FromArgb(255, 255, 255); // Replace Green with this colour
				Color s3 = Color.FromArgb(0, 0, 0); // Replace Blue with this colour

				var c = new SafeColor();
				c.R = ((c1.R * s1.R / 255) + (c1.G * s2.R / 255) + (c1.B * s3.R / 255));
				c.G = ((c1.R * s1.G / 255) + (c1.G * s2.G / 255) + (c1.B * s3.G / 255));
				c.B = ((c1.R * s1.B / 255) + (c1.G * s2.B / 255) + (c1.B * s3.B / 255));

				return c;
			});

			pictureBox1.Image = outImage;
		}

		private void numOpacity_ValueChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			ColorDialog cd = new ColorDialog();
			var res = cd.ShowDialog();
			if (res == System.Windows.Forms.DialogResult.OK)
			{
				this.applyColour = cd.Color;
				this.UpdatePreview();
			}
		}

		private void btnRedReplace_Click(object sender, EventArgs e)
		{
			/*
			ColorDialog cd = new ColorDialog();
			var res = cd.ShowDialog();
			if (res == System.Windows.Forms.DialogResult.OK)
			{
				this.applyColour = cd.Color;
				this.UpdatePreview();
			}
			*/
			//this.applyColour = cd.Color;
			this.UpdatePreview();
		}

		//private void Form1_Paint(object sender, PaintEventArgs e)
		//{
		//    e.Graphics.DrawImage(this.processedImage, new Point(0, 0));
		//}
	}
}

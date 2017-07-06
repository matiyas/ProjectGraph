﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Windows.Controls;
using System.Linq;
using System.Collections;

namespace Projekt_LGiM
{
    public partial class MainWindow : Window
    {
        private byte[] pixs, tmpPixs;
        private Rysownik rysownik;
        private double dpi;
        private Size canvasSize;
        private List<DenseVector> bryla, brylaMod;
        private List<Point> punktyMod;
        private Point srodek;

        public MainWindow()
        {
            InitializeComponent();
            
            SliderRotacjaX.Minimum = SliderRotacjaY.Minimum = SliderRotacjaZ.Minimum = -200 * Math.PI;
            SliderRotacjaX.Maximum = SliderRotacjaY.Maximum = SliderRotacjaZ.Maximum =  200 * Math.PI;

            dpi = 96;

            // Poczekanie na załadowanie się ramki i obliczenie na podstawie 
            // jej rozmiaru rozmiaru tablicy przechowującej piksele.
            Loaded += delegate
            {
                canvasSize.Width = RamkaEkran.ActualWidth;
                canvasSize.Height = RamkaEkran.ActualHeight;

                srodek.X = canvasSize.Width / 2;
                srodek.Y = canvasSize.Height / 2;

                pixs    = new byte[(int)(4 * canvasSize.Width * canvasSize.Height)];
                tmpPixs = new byte[(int)(4 * canvasSize.Width * canvasSize.Height)];

                rysownik = new Rysownik(ref tmpPixs, (int)canvasSize.Width, (int)canvasSize.Height);

                bryla = new List<DenseVector>
                {
                    new DenseVector(new double[] { -100, -100,   0 }),
                    new DenseVector(new double[] {  100, -100,   0 }),
                    new DenseVector(new double[] {  100,  100,   0 }),
                    new DenseVector(new double[] { -100,  100,   0 }),
                    new DenseVector(new double[] { -100, -100, 200 }),
                    new DenseVector(new double[] {  100, -100, 200 }),
                    new DenseVector(new double[] {  100,  100, 200 }),
                    new DenseVector(new double[] { -100,  100, 200 })
                };
                RysujNaEkranie(bryla);
            };
        }

        public void RysujNaEkranie(List<DenseVector> bryla)
        {
            rysownik.CzyscEkran();
            punktyMod = Przeksztalcenie3d.RzutPerspektywiczny(bryla, 500, srodek.X, srodek.Y);

            for (int i = 0; i < 4; ++i)
            {
                rysownik.RysujLinie(punktyMod[i], punktyMod[(i + 1) % 4]);              // Przód
                rysownik.RysujLinie(punktyMod[i + 4], punktyMod[((i + 1) % 4) + 4]);    // Tył
                rysownik.RysujLinie(punktyMod[i], punktyMod[i + 4]);                    // Bok
            }

            Ekran.Source = BitmapSource.Create((int)canvasSize.Width, (int)canvasSize.Height, dpi, dpi, 
                PixelFormats.Bgra32, null, tmpPixs, 4 * (int)canvasSize.Width);
        }

        private void SliderTranslacja_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            brylaMod = Przeksztalcenie3d.Translacja(bryla, SliderTranslacjaX.Value, SliderTranslacjaY.Value, SliderTranslacjaZ.Value);
            RysujNaEkranie(brylaMod);
        }

        private void SliderRotacja_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            brylaMod = Przeksztalcenie3d.Rotacja(bryla, SliderRotacjaX.Value, SliderRotacjaY.Value, SliderRotacjaZ.Value);
            RysujNaEkranie(brylaMod);
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            bryla = brylaMod;

            foreach (object slider in GridTranslacja.Children)
            {
                if(slider is Slider)
                    (slider as Slider).Value = 0;
            }

            foreach (object slider in GridRotacja.Children)
            {
                if (slider is Slider)
                    (slider as Slider).Value = 0;
            }

            foreach (object slider in GridSkalowanie.Children)
            {
                if (slider is Slider)
                    (slider as Slider).Value = 0;
            }

        }
    }
}

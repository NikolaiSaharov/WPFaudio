using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace AudioPlayerWPF
{
    public partial class MainWindow : Window
    {
        private List<string> audioFiles;
        private int currentIndex;
        private bool isRepeatEnabled;
        private bool isShuffleEnabled;
        private Random random;
        private List<string> playHistory;
        private DispatcherTimer positionTimer;

        public MainWindow()
        {
            InitializeComponent();
            audioFiles = new List<string>();
            currentIndex = 0;
            isRepeatEnabled = false;
            isShuffleEnabled = false;
            random = new Random();
            playHistory = new List<string>();
            positionTimer = new DispatcherTimer();
            positionTimer.Interval = TimeSpan.FromSeconds(1);
            positionTimer.Tick += PositionTimer_Tick;
            mediaElement.MediaEnded += MediaElement_MediaEnded;
        }
        private bool PlayingMuz = false;
        private TimeSpan lastPosition = TimeSpan.Zero;
        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false,
                Title = "Выберите папку с музыкой"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string selectedFolder = dialog.FileName;
                string[] newAudioFiles = Directory.GetFiles(selectedFolder, "*.mp3", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(selectedFolder, "*.m4a", SearchOption.AllDirectories))
                    .Concat(Directory.GetFiles(selectedFolder, "*.wav", SearchOption.AllDirectories))
                    .ToArray();

                audioFiles = newAudioFiles.ToList(); 

                if (audioFiles.Count > 0)
                {
                    PlayAudio(0);
                }
            }
        }

        private void PlayAudio(int index)
        {
            if (index >= 0 && index < audioFiles.Count)
            {
                currentIndex = index;
                mediaElement.Source = new Uri(audioFiles[currentIndex], UriKind.Absolute);
                mediaElement.Play();
                playHistory.Add(audioFiles[currentIndex]);
                UpdatePositionTimer();
            }
        }

        private void ShuffleAudioFiles()
        {
            int n = audioFiles.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                string value = audioFiles[k];
                audioFiles[k] = audioFiles[n];
                audioFiles[n] = value;
            }
            PlayAudio(0);
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (isRepeatEnabled)
            {
                PlayAudio(currentIndex);
            }
            else if (isShuffleEnabled)
            {
                ShuffleAudioFiles();
            }
            else
            {
                PlayAudio((currentIndex + 1) % audioFiles.Count);
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement.Source != null)
            {
                if (mediaElement.CanPause && PlayingMuz)
                {
                    mediaElement.Pause();
                    PlayingMuz = false;
                }
                else
                {
                    mediaElement.Play();
                    PlayingMuz = true;
                }
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (isShuffleEnabled)
            {
                ShuffleAudioFiles();
            }
            else
            {
                PlayAudio((currentIndex - 1 + audioFiles.Count) % audioFiles.Count);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (isShuffleEnabled)
            {
                ShuffleAudioFiles();
            }
            else
            {
                PlayAudio((currentIndex + 1) % audioFiles.Count);
            }
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            isRepeatEnabled = !isRepeatEnabled;
            RepeatButton.Content = isRepeatEnabled ? "Повтор: Вкл" : "Повтор: Выкл";
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            isShuffleEnabled = !isShuffleEnabled;
            ShuffleButton.Content = isShuffleEnabled ? "Перемешивание: Вкл" : "Перемешивание: Выкл";
            if (isShuffleEnabled && audioFiles.Count > 0)
            {
                ShuffleAudioFiles();
            }
        }

        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                PositionSlider.Value = mediaElement.Position.TotalSeconds;
                PositionSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;

                TimeSpan elapsedTime = mediaElement.Position;
                CurrentTimeTextBlock.Text = string.Format("{0:mm\\:ss}", elapsedTime);

                TimeSpan remainingTime = mediaElement.NaturalDuration.TimeSpan - elapsedTime;
                RemainingTimeTextBlock.Text = string.Format("-{0:mm\\:ss}", remainingTime);
            }
        }

        private void UpdatePositionTimer()
        {
            positionTimer.Stop();
            positionTimer.Start();
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                TimeSpan newPosition = TimeSpan.FromSeconds(e.NewValue);
                mediaElement.Position = newPosition;
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = e.NewValue / 100.0;
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            PlayHistoryWindow Window1 = new PlayHistoryWindow(playHistory);
            Window1.ShowDialog();
        }
    }
}

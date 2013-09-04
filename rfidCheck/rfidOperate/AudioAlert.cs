using System;
using System.Collections.Generic;
using System.Text;
using System.Media;

namespace RfidReader
{
    public class AudioAlert
    {
        public static void PlayAlert()
        {
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = "alert.wav";
            player.Play();
        }
    }
}

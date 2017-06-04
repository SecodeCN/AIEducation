using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.Text;
using System;
using System.IO;
using NAudio;
using NAudio.Wave;

public class MonoTTS : MonoBehaviour
{
    private static SpeechModelType SpeechModel = null;

    public AudioSource Source;

    void Start()
    {
        SpeechModel = new SpeechModelType();
        StartCoroutine(InitAccessToken());

        if (Source == null)
        {
            try
            {
                this.gameObject.AddComponent<AudioSource>();
            }
            catch { }
            Source = this.GetComponent<AudioSource>();
        }

        Say("小朋友，你好，欢迎来到AI教学体验馆！来玩个小游戏吧，看，前方是由多个正方体随机拼接而成的立体模型，能看出它是由多少个正方体拼接而成的么?那就大声的说出来吧~~~");
    }

    IEnumerator InitAccessToken()
    {
        var url = "https://openapi.baidu.com/oauth/2.0/token";
        var data = "grant_type=client_credentials&client_id=" + SpeechModel.APIKey + "&client_secret=" + SpeechModel.APISecretKey;
        url += "?" + data;
        WWW www = new WWW(url);
        yield return www;
        if (www.error != null)
        {
            print(www.error);
        }
        else
        {
            try
            {
                var result = www.text;
                var jd = JsonMapper.ToObject(result);
                SpeechModel.APIAccessToken = jd["access_token"].ToString();
            }
            catch (System.Exception ex)
            {
                print(ex);
            }
        }
        www.Dispose();
    }

    public List<string> CurrentSaying = new List<string>();

    void Update()
    {
        if (CurrentSaying.Count > 0 && !string.IsNullOrEmpty(SpeechModel.APIAccessToken))
        {
            StartCoroutine(ToSay(CurrentSaying[0]));
            CurrentSaying.Remove(CurrentSaying[0]);
        }
    }

    public void Say(string hello)
    {
        CurrentSaying.Add(hello);
    }

    IEnumerator ToSay(string hello)
    {
        string requestStr = string.Format("http://tsn.baidu.com/text2audio?tex={0}&lan={1}&per={2}&ctp={3}&cuid={4}&tok={5}&spd={6}&pit={7}&vol={8}",
               UrlEncode(hello), SpeechModel.APILanguage, SpeechModel.APIPerson, SpeechModel.APIClientType, SpeechModel.APIID, SpeechModel.APIAccessToken, SpeechModel.APISpeed, SpeechModel.APIPitch, SpeechModel.APIVolume);

        WWW www = new WWW(requestStr);
        yield return www;
        if (www.error != null)
        {
            print(www.error);
        }
        else
        {
            try
            {
                Source.clip = FromMp3Data(www.bytes);
                Source.Play();
            }
            catch (System.Exception ex)
            {
                print(ex);
            }

        }
        www.Dispose();
    }

    public static string UrlEncode(string str)
    {
        StringBuilder sb = new StringBuilder();
        byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
        for (int i = 0; i < byStr.Length; i++)
        {
            sb.Append(@"%" + Convert.ToString(byStr[i], 16));
        }

        return (sb.ToString());
    }

    public class SpeechModelType
    {
        public string APIID { get; set; }
        public string APIKey { get; set; }
        public string APISecretKey { get; set; }
        public string APIAccessToken { get; set; }

        public string APILanguage { get; set; }
        public string APIRecord { get; set; }
        public string APIFormat { get; set; }
        public string APIFrequency { get; set; }

        public string APIClientType { get; set; }
        public string APISpeed { get; set; }
        public string APIPitch { get; set; }
        public string APIVolume { get; set; }
        public string APIPerson { get; set; }

        public SpeechModelType()
        {
            APIID = "9655027"; // can be anything here, just be unique anyway
            APIKey = "jwj2kAfSOtYm3mGmQfwD3fOd"; // Your key
            APISecretKey = "GtOaEu9xz2zDsPHc3iLMK9GbAirAOit7"; // Your secret key
            APILanguage = "zh"; // language
            APIRecord = ""; // recorded audio
            APIFormat = "wav"; // audio format
            APIFrequency = "16000"; // Hz

            APIClientType = "1";
            APISpeed = "4";
            APIPitch = "5";
            APIVolume = "9";
            APIPerson = "4";
        }
    }

    public static AudioClip FromMp3Data(byte[] data)
    {
        // Load the data into a stream
        MemoryStream mp3stream = new MemoryStream(data);
        // Convert the data in the stream to WAV format
        Mp3FileReader mp3audio = new Mp3FileReader(mp3stream);
        WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(mp3audio);
        // Convert to WAV data
        WAV wav = new WAV(AudioMemStream(waveStream).ToArray());
        Debug.Log(wav);
        AudioClip audioClip = AudioClip.Create("testSound", wav.SampleCount, 1, wav.Frequency, false);
        audioClip.SetData(wav.LeftChannel, 0);
        // Return the clip
        return audioClip;
    }

    private static MemoryStream AudioMemStream(WaveStream waveStream)
    {
        MemoryStream outputStream = new MemoryStream();
        using (WaveFileWriter waveFileWriter = new WaveFileWriter(outputStream, waveStream.WaveFormat))
        {
            byte[] bytes = new byte[waveStream.Length];
            waveStream.Position = 0;
            waveStream.Read(bytes, 0, Convert.ToInt32(waveStream.Length));
            waveFileWriter.Write(bytes, 0, bytes.Length);
            waveFileWriter.Flush();
        }
        return outputStream;
    }

    /* From http://answers.unity3d.com/questions/737002/wav-byte-to-audioclip.html */
    public class WAV
    {
        // convert two bytes to one float in the range -1 to 1
        static float bytesToFloat(byte firstByte, byte secondByte)
        {
            // convert two bytes to one short (little endian)
            short s = (short)((secondByte << 8) | firstByte);
            // convert to range from -1 to (just below) 1
            return s / 32768.0F;
        }

        static int bytesToInt(byte[] bytes, int offset = 0)
        {
            int value = 0;
            for (int i = 0; i < 4; i++)
            {
                value |= ((int)bytes[offset + i]) << (i * 8);
            }
            return value;
        }
        // properties
        public float[] LeftChannel { get; internal set; }
        public float[] RightChannel { get; internal set; }
        public int ChannelCount { get; internal set; }
        public int SampleCount { get; internal set; }
        public int Frequency { get; internal set; }

        public WAV(byte[] wav)
        {

            // Determine if mono or stereo
            ChannelCount = wav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels

            // Get the frequency
            Frequency = bytesToInt(wav, 24);

            // Get past all the other sub chunks to get to the data subchunk:
            int pos = 12;   // First Subchunk ID from 12 to 16

            // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
            while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
            {
                pos += 4;
                int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
                pos += 4 + chunkSize;
            }
            pos += 8;

            // Pos is now positioned to start of actual sound data.
            SampleCount = (wav.Length - pos) / 2;     // 2 bytes per sample (16 bit sound mono)
            if (ChannelCount == 2) SampleCount /= 2;        // 4 bytes per sample (16 bit stereo)

            // Allocate memory (right will be null if only mono sound)
            LeftChannel = new float[SampleCount];
            if (ChannelCount == 2) RightChannel = new float[SampleCount];
            else RightChannel = null;

            // Write to double array/s:
            int i = 0;
            while (pos < wav.Length)
            {
                LeftChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
                pos += 2;
                if (ChannelCount == 2)
                {
                    RightChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
                    pos += 2;
                }
                i++;
            }
        }

        public override string ToString()
        {
            return string.Format("[WAV: LeftChannel={0}, RightChannel={1}, ChannelCount={2}, SampleCount={3}, Frequency={4}]", LeftChannel, RightChannel, ChannelCount, SampleCount, Frequency);
        }
    }
}


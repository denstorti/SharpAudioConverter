using NAudio.Utils;
using NAudio.Wave;
using System;
using System.IO;

namespace Converter
{
    public class CustomAudioConverter : IAudioConverter
    {

        public byte[] CustomConversionToLinear(byte[] audio)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            bool resample8KHz16bit = false;

            MemoryStream inMem = new MemoryStream(audio);

            byte[] output;
            int outputLength = 0;

            String traceabilityId = Guid.NewGuid().ToString() + ": ";
            log.Info(traceabilityId + "Start CustomConversionToLinear with audio[" + audio.Length + "]. resample8KHz16bit = " + resample8KHz16bit);
            log.Info(traceabilityId + "First bytes of audio: " + audio[0] + " " + audio[1] + " " + audio[2] + " " + audio[3]);

            switch (AudioType.CheckAudioType(audio))
            {
                case AudioTypeEnum.WAV:
                    log.Info(traceabilityId + "File is WAV");

                    if (!resample8KHz16bit)
                    {
                        output = inMem.ToArray();
                        outputLength = output.Length;
                    }
                    else
                    {
                        byte[] fileBytes;
                        using (var wfr = new WaveFileReader(inMem))
                        {
                            using (var pcmStream = new WaveFormatConversionStream(new WaveFormat(8000, 16, wfr.WaveFormat.Channels), wfr))
                            {
                                fileBytes = new byte[pcmStream.Length];
                                pcmStream.Read(fileBytes, 0, Convert.ToInt32(pcmStream.Length));
                                output = fileBytes;
                                outputLength = output.Length;

                            }
                        }
                    }
                    break;

                case AudioTypeEnum.MP3:
                    log.Info(traceabilityId + "File is MP3");
                    output = ConvertMP3toWAV(inMem, resample8KHz16bit);
                    outputLength = output.Length;
                    break;

                case AudioTypeEnum.WMA:
                    log.Info(traceabilityId + "File is WMA");
                    output = ConvertWMAtoWAV(inMem, resample8KHz16bit);
                    outputLength = output.Length;
                    break;

                default:
                case AudioTypeEnum.Unknown:
                    log.Warn(traceabilityId + "File is UNKOWN");

                    output = inMem.ToArray();
                    outputLength = output.Length;
                    break;
            }
            
            log.Info(traceabilityId + "Finished: output length is " + outputLength);
            return output;
        }

        public static byte[] ConvertMP3toWAV(MemoryStream inMem, bool resample8KHz16bit = false)
        {
            /* its a mp3*/
            MemoryStream outMem = new MemoryStream();

            using (var reader = new Mp3FileReader(inMem))
            {
                if (!resample8KHz16bit)
                {
                    WriteWavFileToStream(outMem, reader);
                    return outMem.ToArray();
                }
                else
                {
                    var outFormat = new WaveFormat(8000, 16, reader.WaveFormat.Channels);

                    using (var resampler = new MediaFoundationResampler(reader, outFormat))
                    {
                        // resampler.ResamplerQuality = 60;
                        IWaveProvider sourceProvider = (IWaveProvider)resampler;
                        long outputLength = 0;
                        var buffer = new byte[sourceProvider.WaveFormat.AverageBytesPerSecond * 4];
                        while (true)
                        {
                            int bytesRead = sourceProvider.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0)
                            {
                                // end of source provider
                                break;
                            }
                            outputLength += bytesRead;
                            if (outputLength > Int32.MaxValue)
                            {
                                throw new InvalidOperationException("WAV File cannot be greater than 2GB. Check that sourceProvider is not an endless stream.");
                            }
                            outMem.Write(buffer, 0, bytesRead);
                        }

                        return outMem.ToArray();
                    }

                }
            }
        }

        //this method is present on NAudio 1.8+, but it is not in 1.7. In order to maintain compatibility with SS11.2 it was copied here from NAudio source code.
        public static void WriteWavFileToStream(Stream outStream, IWaveProvider sourceProvider)
        {   
            using (var writer = new WaveFileWriter(new IgnoreDisposeStream(outStream), sourceProvider.WaveFormat))
            {
                var buffer = new byte[sourceProvider.WaveFormat.AverageBytesPerSecond * 4];
                while (true)
                {
                    var bytesRead = sourceProvider.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // end of source provider
                        outStream.Flush();
                        break;
                    }

                    writer.Write(buffer, 0, bytesRead);
                }
            }
        }

        /// <summary>
        /// PROBLEMS WITH THE NEW VERSION OF WMA DOES NOT MAKE THIS CODE WORK. THERE IS NO CONSTRUCTOR WITH STREAM INPUT in NAUDIO.WMA
        /// </summary>
        /// <param name="audio"></param>
        /// <returns></returns>
        public static byte[] ConvertWMAtoWAV(MemoryStream inMem, bool resample8KHz16bit = false)
        {
            /* its a wma */
            //MemoryStream outMem = new MemoryStream();
            //using (var reader = new WMAFileReader(inMem))
            //{
            //    var outFormat = new WaveFormat(8000, 16, 1);
            //    using (var resampler = new MediaFoundationResampler(reader, outFormat))
            //    {
            //        // resampler.ResamplerQuality = 60;
            //        IWaveProvider sourceProvider = (IWaveProvider)resampler;
            //        long outputLength = 0;
            //        var buffer = new byte[sourceProvider.WaveFormat.AverageBytesPerSecond * 4];
            //        while (true)
            //        {
            //            int bytesRead = sourceProvider.Read(buffer, 0, buffer.Length);
            //            if (bytesRead == 0)
            //            {
            //                // end of source provider
            //                break;
            //            }
            //            outputLength += bytesRead;
            //            if (outputLength > Int32.MaxValue)
            //            {
            //                throw new InvalidOperationException("WAV File cannot be greater than 2GB. Check that sourceProvider is not an endless stream.");
            //            }
            //            outMem.Write(buffer, 0, bytesRead);
            //        }

            //        return outMem.ToArray();
            //    }
            //}

            throw new NotImplementedException();
        }
    }
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace TeconMoon_s_WiiVC_Injector
{
    namespace Utils
    {
        class Draw
        {
            // Get a adjusted font which is fit to specified width and height.
            // Using Graphics's method.
            // Modified from MSDN: https://msdn.microsoft.com/en-us/library/bb986765.aspx
            static public Font GetGraphicAdjustedFont(
                Graphics g,
                string graphicString,
                Font originalFont,
                int containerWidth,
                int containerHeight,
                int maxFontSize,
                int minFontSize,
                StringFormat stringFormat,
                bool smallestOnFail
                )
            {
                Font testFont = null;
                // We utilize MeasureString which we get via a control instance           
                for (int adjustedSize = maxFontSize; adjustedSize >= minFontSize; adjustedSize--)
                {
                    testFont = new Font(originalFont.Name, adjustedSize, originalFont.Style);

                    // Test the string with the new size
                    SizeF adjustedSizeNew = g.MeasureString(
                        graphicString,
                        testFont,
                        containerWidth,
                        stringFormat);

                    if (containerWidth > Convert.ToInt32(adjustedSizeNew.Width) &&
                        containerHeight > Convert.ToInt32(adjustedSizeNew.Height))
                    {
                        // Good font, return it
                        return testFont;
                    }
                }

                // If you get here there was no fontsize that worked
                // return minimumSize or original?
                if (smallestOnFail)
                {
                    return testFont;
                }
                else
                {
                    return originalFont;
                }
            }

            // Get a adjusted font which is fit to specified width and height.
            // Using TextRenderer's method.
            static public Font GetTextRendererAdjustedFont(
                Graphics g,
                string text,
                Font originalFont,
                int containerWidth,
                int containerHeight,
                int maxFontSize,
                int minFontSize,
                TextFormatFlags flags,
                bool smallestOnFail
                )
            {
                Font testFont = null;
                // We utilize MeasureText which we get via a control instance           
                for (int adjustedSize = maxFontSize; adjustedSize >= minFontSize; adjustedSize--)
                {
                    testFont = new Font(originalFont.Name, adjustedSize, originalFont.Style);

                    // Test the string with the new size
                    Size adjustedSizeNew = TextRenderer.MeasureText(
                        g,
                        text,
                        testFont,
                        new Size(containerWidth, containerHeight),
                        flags);

                    if (containerWidth > adjustedSizeNew.Width &&
                        containerHeight > adjustedSizeNew.Height)
                    {
                        // Good font, return it
                        return testFont;
                    }
                }

                // If you get here there was no fontsize that worked
                // return minimumSize or original?
                if (smallestOnFail)
                {
                    return testFont;
                }
                else
                {
                    return originalFont;
                }
            }

            // Draw a string in a specified rectangle with
            // a specified font with max font size that can
            // fit to the rectangle.
            static public void ImageDrawString(
                ref Bitmap bitmap,
                string s,
                Rectangle rectangle,
                Font font,
                Color foreColor,
                bool adjustedFontByTextRenderer,
                bool drawStringByTextRenderer
                )
            {
                StringFormat stringFormat = StringFormat.GenericDefault;

                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    TextFormatFlags flags = TextFormatFlags.HorizontalCenter
                        | TextFormatFlags.VerticalCenter
                        | TextFormatFlags.WordBreak;

                    if (!adjustedFontByTextRenderer)
                    {
                        font = GetGraphicAdjustedFont(
                            graphics,
                            s,
                            font,
                            rectangle.Width,
                            rectangle.Height,
                            100, 8,
                            stringFormat,
                            true);
                    }
                    else
                    {
                        // Can't get the correct word break output
                        // if we use GetGraphicAdjustedFont.
                        // But it's really more slower than 
                        // GetGraphicAdjustedFont.
                        font = GetTextRendererAdjustedFont(
                            graphics,
                            s,
                            font,
                            rectangle.Width,
                            rectangle.Height,
                            64, 8,
                            flags,
                            true);
                    }

                    if (!drawStringByTextRenderer)
                    {
                        SizeF sizeF = graphics.MeasureString(s, font, rectangle.Width);

                        RectangleF rectF = new RectangleF(
                            rectangle.X + (rectangle.Width - sizeF.Width) / 2,
                            rectangle.Y + (rectangle.Height - sizeF.Height) / 2,
                            sizeF.Width,
                            sizeF.Height);

                        graphics.DrawString(
                            s,
                            font,
                            new SolidBrush(foreColor),
                            rectF,
                            stringFormat);
                    }
                    else
                    {
                        // Poor draw performance, both for speed and output result.
                        Size size = TextRenderer.MeasureText(
                            graphics,
                            s,
                            font,
                            new Size(rectangle.Width, rectangle.Height),
                            flags);

                        Rectangle rect = new Rectangle(
                            rectangle.X + (rectangle.Width - size.Width) / 2,
                            rectangle.Y + (rectangle.Height - size.Height) / 2,
                            size.Width,
                            size.Height);

                        TextRenderer.DrawText(
                            graphics,
                            s,
                            font,
                            rect,
                            foreColor,
                            flags);
                    }
                }
            }
        }

        class StringEx
        {
            // Check if the input byte array is GB2312 encoded.
            static public bool IsGB2312EncodingArray(byte[] b)
            {
                int i = 0;
                while (i < b.Length)
                {
                    if (b[i] <= 127)
                    {
                        ++i;
                        continue;
                    }

                    if (b[i] >= 176 && b[i] <= 247)
                    {
                        if (i == b.Length - 1)
                        {
                            return false;
                        }
                        ++i;

                        if (b[i] < 160 || b[i] > 254)
                        {
                            return false;
                        }

                        ++i;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }

            // Get a probably encoding object for input array.
            static public Encoding GetArrayEncoding(byte[] b)
            {
                if (IsGB2312EncodingArray(b))
                {
                    return Encoding.GetEncoding("GB2312");
                }

                // We assume it is utf8 by default.
                return Encoding.UTF8;
            }

            // Read a string from a binary stream.
            static public string ReadStringFromBinaryStream(BinaryReader reader, long position, bool peek = false)
            {
                long oldPosition = 0;

                if (peek)
                {
                    oldPosition = reader.BaseStream.Position;
                }

                reader.BaseStream.Position = position;
                ArrayList readBuffer = new ArrayList();
                byte b;
                while ((b = reader.ReadByte()) != 0)
                {
                    readBuffer.Add(b);
                }

                if (peek)
                {
                    reader.BaseStream.Position = oldPosition;
                }

                byte[] readBytes = readBuffer.OfType<byte>().ToArray();
                return Encoding.Default.GetString(Encoding.Convert(
                    GetArrayEncoding(readBytes),
                    Encoding.Default,
                    readBytes));
            }
        }

        class TranslationTemplate
        {
            private IniFile TemplateFile
            {
                get;
                set;
            }

            public string TemplateFileName
            {
                get
                {
                    return TemplateFile != null ? TemplateFile.FileName : "";
                }
            }

            private TranslationTemplate(string templateFile)
            {
                TemplateFile = new IniFile(templateFile);
            }

            static public TranslationTemplate LoadTemplate(string templateFilePath)
            {
                return new TranslationTemplate(templateFilePath);
            }

            static public TranslationTemplate CreateTemplate(
                string templateFilePath,
                string appName,
                string defaultLanguageName,
                string version)
            {
                TranslationTemplate template = new TranslationTemplate(templateFilePath);

                template.TemplateFile.CurrentSection = appName;
                template.TemplateFile.WriteStringValue("language", defaultLanguageName);
                template.TemplateFile.WriteStringValue("verion", version);

                return template;
            }

            public void AppendFormTranslation(Form form)
            {
                TemplateFile.CurrentSection = form.Name;
                TemplateFile.WriteStringValue("@Title", form.Text);

                foreach (Control control in form.Controls)
                {
                    AppendControlTranslation(control);
                }
            }

            private void AppendControlTranslation(Control control)
            {
                TemplateFile.WriteStringValue(control.Name, control.Text);

                foreach (Control subControl in control.Controls)
                {
                    AppendControlTranslation(subControl);
                }
            }

            public void LoadFormTranslation(Form form)
            {
                TemplateFile.CurrentSection = form.Name;
                TranslateControl(form, "@Title");

                foreach (Control control in form.Controls)
                {
                    LoadControlTranslation(control);
                }
            }

            private void LoadControlTranslation(Control control)
            {
                TranslateControl(control);

                foreach (Control subControl in control.Controls)
                {
                    LoadControlTranslation(subControl);
                }
            }

            private void TranslateControl(Control control)
            {
                TranslateControl(control, control.Name);
            }

            private void TranslateControl(Control control, string id)
            {
                string translation = TemplateFile.ReadStringValue(id, 1024);

                if (!String.IsNullOrEmpty(translation))
                {
                    control.Text = translation;
                }
            }
        }

        class IniFile
        {
            public string FileName
            {
                get;
                protected set;
            }

            public string CurrentSection
            {
                get;
                set;
            }

            public IniFile(string iniFile)
            {
                FileName = iniFile;
            }

            public bool WriteStringValue(string key, string value)
            {
                return WriteStringValue(CurrentSection, key, value);
            }

            public bool WriteStringValue(string section, string key, string value)
            {
                return Win32Native.WritePrivateProfileString(
                    section, key, value.Replace("\r\n", "\\r\\n"), FileName);
            }

            public string ReadStringValue(string key, int maxLength, string defaultValue = "")
            {
                return ReadStringValue(CurrentSection, key, maxLength, defaultValue);
            }

            public string ReadStringValue(string section, string key, int maxLength, string defaultValue = "")
            {
                StringBuilder buffer = new StringBuilder(maxLength);
                buffer.Length = maxLength;
                string value = buffer.ToString(0, maxLength);
                int length = (int)Win32Native.GetPrivateProfileString(
                    section,
                    key,
                    defaultValue,
                    value,
                    (uint)maxLength,
                    FileName);
                return value.Substring(0, length).Replace("\\r\\n", "\r\n");                
            }

            public string[] GetSections()
            {
                string value = new StringBuilder(65535).ToString(0, 65535);
                if (Win32Native.GetPrivateProfileString(null, null, "", value, 65535, FileName) != 0)
                {
                    return StringsFromMultiString(value);
                }

                return new string[0];
            }

            private static string[] StringsFromMultiString(string s)
            {
                string[] raw = s.Split('\0');
                return raw.Take(raw.Length - 2).ToArray();
             }
        }
    }
}

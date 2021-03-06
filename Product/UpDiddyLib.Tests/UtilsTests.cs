﻿using System;
using Xunit;

namespace UpDiddyLib.Tests
{
    public class UtilsTests
    {
        /* todo: set up "theory" tests in this format?
        [Theory]
        [InlineData("")]
        public void RemoveQueryStringFromUrl_ReturnsValidUrl(string url)
        {
            Assert.True(true);
        }
        */

        [Fact]
        public void RemoveQuerStringFromUrl_NoParameters()
        {
            string modifiedUrl = UpDiddyLib.Helpers.Utils.RemoveQueryStringFromUrl("https://www.google.com/");
            Assert.True(Uri.IsWellFormedUriString(modifiedUrl, UriKind.RelativeOrAbsolute));
        }

        [Fact]
        public void RemoveQuerStringFromUrl_MultipleQuestionMarks()
        {
            string modifiedUrl = UpDiddyLib.Helpers.Utils.RemoveQueryStringFromUrl("https://www.google.com/?q=somevalue?");
            Assert.True(Uri.IsWellFormedUriString(modifiedUrl, UriKind.RelativeOrAbsolute));
        }

        [Fact]
        public void RemoveQuerStringFromUrl_QuestionMarkWithoutParameters()
        {
            string modifiedUrl = UpDiddyLib.Helpers.Utils.RemoveQueryStringFromUrl("https://www.google.com/?");
            Assert.True(Uri.IsWellFormedUriString(modifiedUrl, UriKind.RelativeOrAbsolute));
        }

        [Fact]
        public void IsValidTextFile_DocFile()
        {
            Assert.True(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.doc"));
        }

        [Fact]
        public void IsValidTextFile_DocxFile()
        {
            Assert.True(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.docx"));
        }

        [Fact]
        public void IsValidTextFile_OdtFile()
        {
            Assert.True(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.odt"));
        }

        [Fact]
        public void IsValidTextFile_PdfFile()
        {
            Assert.True(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.pdf"));
        }

        [Fact]
        public void IsValidTextFile_RtfFile()
        {
            Assert.True(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.odt"));
        }

        [Fact]
        public void IsValidTextFile_TexFile()
        {
            Assert.True(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.tex"));
        }

        [Fact]
        public void IsValidTextFile_TxtFile()
        {
            Assert.True(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.txt"));
        }

        [Fact]
        public void IsValidTextFile_WksFile()
        {
            Assert.True(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.wks"));
        }

        [Fact]
        public void IsValidTextFile_WpsFile()
        {
            Assert.True(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.wps"));
        }

        [Fact]
        public void IsValidTextFile_WpdFile()
        {
            Assert.True(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.wpd"));
        }

        [Fact]
        public void IsValidTextFile_PngFile()
        {
            Assert.False(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.png"));
        }

        [Fact]
        public void IsValidTextFile_JpgFile()
        {
            Assert.False(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.jpg"));
        }

        [Fact]
        public void IsValidTextFile_JpegFile()
        {
            Assert.False(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.jpeg"));
        }

        [Fact]
        public void IsValidTextFile_Mp3File()
        {
            Assert.False(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.mp3"));
        }

        [Fact]
        public void IsValidTextFile_Mp4File()
        {
            Assert.False(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.mp4"));
        }

        [Fact]
        public void IsValidTextFile_XlsFile()
        {
            Assert.False(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.xls"));
        }

        [Fact]
        public void IsValidTextFile_PptFile()
        {
            Assert.False(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.ppt"));
        }

        [Fact]
        public void IsValidTextFile_FileWithSpaces()
        {
            Assert.True(UpDiddyLib.Helpers.Utils.IsValidTextFile("This is a testfile.doc"));
        }

        [Fact]
        public void IsValidTextFile_FileWithMultiplePeriods()
        {
            Assert.True(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfile.othertest.doc"));
        }

        [Fact]
        public void IsValidTextFile_NoFileName()
        {
            Assert.False(UpDiddyLib.Helpers.Utils.IsValidTextFile(""));
        }

        [Fact]
        public void IsValidTextFile_NoPeriodInFileName()
        {
            Assert.False(UpDiddyLib.Helpers.Utils.IsValidTextFile("testfiledoc"));
        }

        [Fact]
        public void IsValidTextFile_NullFileName()
        {
            Assert.False(UpDiddyLib.Helpers.Utils.IsValidTextFile(null));
        }

        [Fact]
        public void ToTitleCase_EmptyString()
        {
            Assert.Equal(string.Empty, UpDiddyLib.Helpers.Utils.ToTitleCase(string.Empty));
        }

        [Fact]
        public void ToTitleCase_NullInput()
        {
            Assert.Equal(string.Empty, UpDiddyLib.Helpers.Utils.ToTitleCase(null));
        }

        [Fact]
        public void ToTitleCase_John()
        {
            Assert.Equal("John", UpDiddyLib.Helpers.Utils.ToTitleCase("john"));
        }

        [Fact]
        public void ToTitleCase_JohnLowercase()
        {
            Assert.NotEqual("john", UpDiddyLib.Helpers.Utils.ToTitleCase("john"));
        }

        [Fact]
        public void ToTitleCase_NameWithHyphen()
        {
            Assert.Equal("Mary-Beth", UpDiddyLib.Helpers.Utils.ToTitleCase("mary-beth"));
        }

        [Fact]
        public void ToTitleCase_NameWithApostrophe()
        {
            Assert.Equal("D'artagnan", UpDiddyLib.Helpers.Utils.ToTitleCase("d'artagnan"));
        }

        [Fact]
        public void ToTitleCase_Address()
        {
            Assert.Equal("123 Test Drive", UpDiddyLib.Helpers.Utils.ToTitleCase("123 test drive"));
        }

        [Fact]
        public void ToTitleCase_DotNet()
        {
            Assert.Equal(".Net", UpDiddyLib.Helpers.Utils.ToTitleCase(".net"));
        }

        [Fact]
        public void ToTitleCase_Parens()
        {
            Assert.Equal("Sarbanes Oxley (Sox)", UpDiddyLib.Helpers.Utils.ToTitleCase("SARBANES OXLEY (SOX)"));
        }
    }
}
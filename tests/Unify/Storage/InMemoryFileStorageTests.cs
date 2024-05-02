using CNCO.Unify;
using CNCO.Unify.Storage;
using System.Text;

namespace UnifyTests.Storage {
    public class InMemoryFileStorageTests {
        private InMemoryFileStorage _fileStorage;

        [SetUp]
        public void SetUp() {
            Runtime.Current.Configuration.SuppressFileStorageExceptions = false;
            _fileStorage = new InMemoryFileStorage();
        }

        [TearDown]
        public void TearDown() {
            _fileStorage.Dispose();
        }

        #region Valid cases

        #region Append
        [TestCase("myFile.txt", "Random data 1234!")]
        [TestCase("my/super/cool/path\\to\\myFile.txt", "Hello hello, I'm sitting in memory!")]
        public void Append_NewFileValidName_CreatesFile(string name, string contents) {
            // Create file
            _fileStorage.Append(name, contents);

            // Check contents
            Assert.Multiple(() => {
                Assert.That(_fileStorage.Exists(name), Is.True);

                Assert.That(_fileStorage.Read(name), Is.EqualTo(contents));
            });
        }

        [TestCase("myFile.txt", "Random data 1234!")]
        [TestCase("my/super/cool/path\\to\\myFile.txt", "Hello hello, I'm sitting in memory!")]
        public void Append_ExistingFileValidName_CreatesFile(string name, string contents) {
            // Create file
            _fileStorage.Write(name, "TestingPrefix!!");

            // Append
            _fileStorage.Append(name, contents);

            // Check contents
            Assert.Multiple(() => {
                Assert.That(_fileStorage.Exists(name), Is.True);

                Assert.That(_fileStorage.Read(name), Is.EqualTo("TestingPrefix!!" + contents));
            });
        }

        [TestCase("myFile.txt", "Random data 1234!")]
        [TestCase("my/Super/cool/path\\to\\myFile.txt", "Hello hello, I'm sitting in memory!")]
        public void AppendBytes_NewFileValidName_CreatesFile(string name, string contents) {
            byte[] bytes = Encoding.UTF8.GetBytes(contents);
            // Create file
            _fileStorage.AppendBytes(name, bytes);

            // Check contents
            Assert.Multiple(() => {
                Assert.That(_fileStorage.Exists(name), Is.True);

                Assert.That(_fileStorage.Read(name), Is.EqualTo(bytes));
            });
        }

        [TestCase("myFile.txt", "Random data 1234!")]
        [TestCase("my/super/cool/path\\to\\myFile.txt", "Hello hello, I'm sitting in memory!")]
        public void AppendBytes_ExistingFileValidName_CreatesFile(string name, string contents) {
            byte[] bytes = Encoding.UTF8.GetBytes(contents);
            byte[] prefix = Encoding.UTF8.GetBytes("TestingPrefix!!");
            // Create file
            _fileStorage.WriteBytes(name, prefix);

            // Append
            _fileStorage.AppendBytes(name, bytes);

            // Check contents
            Assert.Multiple(() => {
                Assert.That(_fileStorage.Exists(name), Is.True);

                Assert.That(_fileStorage.Read(name), Is.EqualTo(prefix.Concat(bytes).ToArray()));
            });
        }
        #endregion

        #region File name validity
        [TestCase("myCaseSensitiveFile")]
        [TestCase("sOmeThINGiMpo/rtANt.HeY")]
        public void CaseSensitivity_CaseSensitive_PreservesCase(string name) {
            _fileStorage.CaseSensitiveFileNames = true;

            // Create
            _fileStorage.Write(name, "TestData");

            Assert.Multiple(() => {
                Assert.That(_fileStorage.Exists(name), "File exists");
                Assert.That(!_fileStorage.Exists(name.ToLower()), "All lowercase file does NOT exist.");

                Assert.That(_fileStorage.Read(name), Is.EqualTo("TestData"));
            });
        }

        [TestCase("myCaseInsensitiveFile")]
        [TestCase("sOmeThINGiMpo/rtANt.HeY")]
        public void CaseSensitivity_CaseInsensitive_IgnoresCase(string name) {
            _fileStorage.CaseSensitiveFileNames = false;

            // Create
            _fileStorage.Write(name, "TestData");

            Assert.Multiple(() => {
                Assert.That(_fileStorage.Exists(name), "File exists");
                Assert.That(_fileStorage.Exists(name.ToLower()), "All lowercase file name exists");

                Assert.That(_fileStorage.Read(name), Is.EqualTo("TestData"));
                Assert.That(_fileStorage.Read(name.ToUpper()), Is.EqualTo("TestData"));
            });
        }

        [TestCase("my<file.txt")]
        [TestCase("my>file.txt")]
        [TestCase("my:file.txt")]
        [TestCase("my\"file.txt")]
        [TestCase("my|file.txt")]
        [TestCase("my?file.txt")]
        [TestCase("my*file.txt")]
        [TestCase("my\0file.txt")]
        [TestCase("my\tfile.txt")]
        public void InvalidFileNames_Ntfs_ThrowsIOException(string name) {
            try {
                _fileStorage.Write(name, "TestData");
                Assert.That(Is.True, Is.False, "Should have thrown");
            } catch (Exception ex) {
                Assert.That(ex, Is.TypeOf<IOException>());
            }
        }

        [TestCase("my<file.txt", false)]
        [TestCase("my>file.txt", false)]
        [TestCase("my:file.txt", false)]
        [TestCase("my\"file.txt", false)]
        [TestCase("my|file.txt", false)]
        [TestCase("my?file.txt", false)]
        [TestCase("my*file.txt", false)]
        [TestCase("my\0file.txt", true)]
        [TestCase("my\tfile.txt", false)]
        public void InvalidFileNames_AllowInvalidNtfsCharacters_ThrowsIOException(string name, bool shouldThrow) {
            try {
                _fileStorage.AllowInvalidNtfsFileNames = true;
                _fileStorage.Write(name, "TestData");
                Assert.That(shouldThrow, Is.False);
            } catch (Exception ex) {
                Assert.Multiple(() => {
                    Assert.That(shouldThrow, Is.True);
                    Assert.That(ex, Is.TypeOf<IOException>());
                });
            }
        }
        #endregion

        [TestCase("myFile.txt")]
        [TestCase("my/super/cool/path\\to\\myFile.txt")]
        public void Delete_ValidName_DeletesFile(string name) {
            // Create
            _fileStorage.Write(name, "TestData");

            // Verify it's there
            Assert.That(_fileStorage.Exists(name), "File was able to be created (setup).");

            // Delete
            _fileStorage.Delete(name);

            Assert.Multiple(() => {
                // Verify
                Assert.That(!_fileStorage.Exists(name), "File doesn't exist");
                Assert.That(() => _fileStorage.Read(name), Throws.Exception.TypeOf<FileNotFoundException>());
            });
        }

        [TestCase("myFile.txt", "/root/path/to/myFile (1).txt")]
        [TestCase("my/super/cool/path\\to\\myFile.txt", "my/other/kind-of/cool/path\\or\\something/I/think.txt")]
        public void Rename_ValidNames_RenamesTheFile(string name, string newName) {
            // Create
            _fileStorage.Write(name, "TestData");

            // Verify it's there
            Assert.That(_fileStorage.Exists(name), "File was able to be created (setup).");

            // Rename
            _fileStorage.Rename(name, newName);

            Assert.Multiple(() => {
                // Verify
                Assert.That(!_fileStorage.Exists(name), "File doesn't exist");
                Assert.That(() => _fileStorage.Read(name), Throws.Exception.TypeOf<FileNotFoundException>());
                Assert.That(_fileStorage.Read(newName), Is.EqualTo("TestData"));
            });
        }

        [TestCase("file.txt")]
        [TestCase("/other/file.dat")]
        [TestCase("more/different/file.webp")]
        public void GetPath_WithDirectory_CombinesPaths(string name) {
            using (InMemoryFileStorage fileStorage = new InMemoryFileStorage("/myDirectory/")) {
                string expected = "/myDirectory/";
                if (name.StartsWith("/"))
                    expected = name;
                else
                    expected += name;

                Assert.That(fileStorage.GetPath(name), Is.EqualTo(expected));
            }
        }
        #endregion
    }
}

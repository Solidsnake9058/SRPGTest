using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace UnityEngine
{
    /// <summary>
    /// PlayerPrefs extensions.
    /// </summary>
    public sealed class PlayerPrefsX
    {
        private PlayerPrefsX() { }

        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public static int GetInt(string key, int defaultValue)
        {
            if (!PlayerPrefs.HasKey(key))
                SetInt(key, defaultValue);
            return PlayerPrefs.GetInt(key);
        }

        public static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        public static float GetFloat(string key, float defaultValue)
        {
            if (!PlayerPrefs.HasKey(key))
                SetFloat(key, defaultValue);
            return PlayerPrefs.GetFloat(key);
        }

        public static void SetDouble(string key, double value)
        {
            PlayerPrefs.SetString(key, value.ToString());
        }

        public static double GetDouble(string key, double defaultValue)
        {
            if (!PlayerPrefs.HasKey(key))
                SetDouble(key, defaultValue);
            return System.Convert.ToDouble(PlayerPrefs.GetString(key));
        }

        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public static string GetString(string key, string defaultValue)
        {
            if (!PlayerPrefs.HasKey(key))
                SetString(key, defaultValue);
            return PlayerPrefs.GetString(key);
        }

        public static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, System.Convert.ToInt32(value));
        }

        public static bool GetBool(string key, bool defaultValue)
        {
            if (!PlayerPrefs.HasKey(key))
                SetBool(key, defaultValue);
            return System.Convert.ToBoolean(PlayerPrefs.GetInt(key));
        }

        public static void SetClass<T>(string key, T value) where T : class
        {
            string stringData = JsonConvert.SerializeObject(value);
#if UNITY_EDITOR
            Debug.Log("SaveData: " + stringData);
            Debug.Log("SaveDataLength: " + stringData.Length);
#endif
            PlayerPrefs.SetString(key, stringData);
        }

        public static T GetClass<T>(string key, T defaultValue = null) where T : class
        {
            if (!PlayerPrefs.HasKey(key))
                SetClass(key, defaultValue);
            return JsonConvert.DeserializeObject<T>(PlayerPrefs.GetString(key));
        }

        public static void SetClassEncrypt<T>(string key, T value) where T : class
        {
            string stringData = JsonConvert.SerializeObject(value);
            byte[] jsonByteArray = Encoding.UTF8.GetBytes(stringData);
            byte[] encryptStream = EncryptDataByAes(jsonByteArray);
            string encryptData = System.Convert.ToBase64String(encryptStream);
#if UNITY_EDITOR
            Debug.Log("SaveData: " + stringData);
            Debug.Log("SaveDataLength: " + encryptData.Length);
#endif
            PlayerPrefs.SetString(key, encryptData);
        }

        public static T GetClassDecrypt<T>(string key, T defaultValue = null) where T : class
        {
            if (!PlayerPrefs.HasKey(key))
                SetClassEncrypt(key, defaultValue);
            return GetClassDecryptMethod<T>(key);
        }

        private static T GetClassDecryptMethod<T>(string key) where T : class
        {
            string encryptData = PlayerPrefs.GetString(key);
            byte[] encryptStream = System.Convert.FromBase64String(encryptData);
            byte[] decryptStream = DecryptDataByAes(encryptStream);
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(decryptStream));
        }

        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        public static void DeleteAllKey()
        {
            PlayerPrefs.DeleteAll();
        }

        public static int FastMask(int value)
        {
            return (~(value * 37));
        }

        public static int FastUnMask(int value)
        {
            return ((~value) / 37);
        }

        public static void PrintBytes(byte[] bytes)
        {
            string text = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                text += bytes[i].ToString();
            }
            Debug.Log(text);
        }

        #region Encrypt & Decrypt
        private const string AesKey = @"B867FB444CECC75E";
        private const string AesIV = @"594D424E4890947A";

        private static byte[] EncryptDataByAes(byte[] org)
        {
            RijndaelManaged aes = GetAes();

            byte[] retval = null;
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptStream.Write(org, 0, org.Length);
                    cryptStream.FlushFinalBlock();
                    retval = memoryStream.ToArray();
                }
            }
            return retval;
        }

        private static byte[] DecryptDataByAes(byte[] org)
        {
            RijndaelManaged aes = GetAes();

            var planeText = new byte[org.Length];
            using (var memoryStream = new MemoryStream(org))
            {
                using (var cryptStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cryptStream.Read(planeText, 0, planeText.Length);
                }
            }
            return planeText;
        }

        private static RijndaelManaged GetAes()
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.BlockSize = 128;
            aes.KeySize = 128;
            aes.Padding = PaddingMode.Zeros;
            aes.Mode = CipherMode.CBC;
            aes.Key = Encoding.UTF8.GetBytes(AesKey);
            aes.IV = Encoding.UTF8.GetBytes(AesIV);
            return aes;
        }
        #endregion
    }
}

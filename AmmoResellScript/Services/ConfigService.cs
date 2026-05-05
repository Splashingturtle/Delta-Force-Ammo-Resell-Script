using AmmoResellScript.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace AmmoResellScript.Services
{
    public static class ConfigService
    {
        private const string SettingsFileName = "device-settings.json";
        private static readonly SemaphoreSlim IoLock = new SemaphoreSlim(1, 1);
        public static string GetSettingsPath() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);

        public static async Task<UserModel> LoadDeviceSettingAsync()
        {
            var path = GetSettingsPath();
            UserModel? settings = null;
            await IoLock.WaitAsync();
            try
            {
                if (File.Exists(path))
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(path);
                        var opt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };//设置属性名称不区分大小写
                        settings = JsonSerializer.Deserialize<UserModel>(json, opt);
                        if (settings != null)
                        {
                            return settings;
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        BackCorrupFile(path);//备份损坏的文件
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    //提示文件不存在，返回默认设置
                }
            }
            finally
            {
                IoLock.Release();//释放锁,允许其他线程访问
            }

            if (settings == null)
            {
                settings = new UserModel();
                //保存方法
                await SaveDeviceSettingAsync(settings);
            }

            return settings;
        }

        public static async Task<bool> SaveDeviceSettingAsync(UserModel settings)
        {
            if (settings == null)
            {
                MessageBox.Show("无法保存配置，设置对象为null！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            var path = GetSettingsPath();
            var tempPath = path + ".tmp";//临时文件路径
            await IoLock.WaitAsync();
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });//格式化输出
                await File.WriteAllTextAsync(tempPath, json);
                File.Move(tempPath, path, true);//覆盖写入
                //提示配置保存成功
                MessageBox.Show("配置保存成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                IoLock.Release();//释放锁,允许其他线程访问
                if (File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch
                    {
                        //日志记录异常
                    }
                }
            }
        }

        public static void BackCorrupFile(string originalPath)
        {
            try
            {
                //生成备份路径
                var backupPath = originalPath + ".corrupt" + DateTime.Now.ToString("yyyyMMddHHmmss");
                File.Copy(originalPath, backupPath, true);//拷贝操作
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

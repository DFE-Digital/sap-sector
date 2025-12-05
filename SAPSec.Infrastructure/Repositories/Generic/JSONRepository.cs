using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SAPSec.Core.Interfaces.Repositories.Generic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Repositories.Generic
{
    public class JSONRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly string _filePath;
        private readonly ILogger<JSONRepository<T>> _logger;

        public JSONRepository(ILogger<JSONRepository<T>> logger, IHostEnvironment env)
        {
            _logger = logger ?? throw new ArgumentNullException();
            var basePath = AppContext.BaseDirectory;
            _filePath = Path.Combine(basePath, "Data", "Files");
        }


        public IEnumerable<T> ReadAll()
        {
            try
            {
                var fileData = ReadFile(typeof(T).Name);
                if (!string.IsNullOrWhiteSpace(fileData))
                {
                    return JsonConvert.DeserializeObject<IEnumerable<T>>(fileData) ?? [];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to execute generic readall for {typeof(T).Name}! - {ex.Message}, {ex}");
            }

            return [];
        }

        private string ReadFile(string fileName)
        {
            try
            {
                var fullPath = Path.Combine(_filePath, $"{fileName}.json");
                return System.IO.File.ReadAllText(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to Read file {fileName}! - {ex.Message}, {ex}");
                return string.Empty;
            }
        }
    }
}

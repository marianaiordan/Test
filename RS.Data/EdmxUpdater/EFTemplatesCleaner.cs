using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RS.Data.EdmxUpdater
{
    public class EFTemplatesCleaner
    {
        public void SetTemplatesContent()
        {
            var templatesDirectory = new DirectoryInfo(Configurator.EFTemplatesPath);
            var templates = templatesDirectory.GetFiles(Constants.TemplatesConstants.TemplateNamePattern);

            foreach (var file in templates)
            {
                var lines = File.ReadAllLines(file.FullName).ToList();
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains(Constants.TemplatesConstants.CleanupSetting))
                    {
                        lines.RemoveAt(i);
                        break;
                    }
                }

                File.WriteAllLines(file.FullName, lines.ToArray());
            }
        }
    }
}
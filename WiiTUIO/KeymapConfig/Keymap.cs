using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiiTUIO.Properties;

namespace WiiTUIO
{
    public class Keymap
    {
        public string Filename;

        public Keymap Parent;

        private JObject jsonObj;

        public Keymap(Keymap parent, string filename)
        {
            this.Parent = parent;
            this.Filename = filename;
            if (File.Exists(Settings.Default.keymaps_path + filename))
            {
                StreamReader reader = File.OpenText(Settings.Default.keymaps_path + filename);
                this.jsonObj = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                reader.Close();
            }
            else
            {
                this.jsonObj = new JObject();
                this.jsonObj.Add("Title", "New Keymap");
                save();
            }
        }

        public string getFilename()
        {
            return this.Filename;
        }

        public string getName()
        {
            return this.jsonObj.GetValue("Title").ToString();
        }

        public void setName(string name)
        {
            this.jsonObj.Remove("Title");
            this.jsonObj.Add("Title", name);

            //Needed to update the name in layout chooser because its names are stored in a different file
            KeymapSettings settings = new KeymapSettings(Settings.Default.keymaps_config);
            if(settings.isInLayoutChooser(this))
            {
                settings.removeFromLayoutChooser(this);
                settings.addToLayoutChooser(this);
            }

            save();
        }

        private void save()
        {
            File.WriteAllText(Settings.Default.keymaps_path + this.Filename, this.jsonObj.ToString());
        }

        public void setConfigFor(int controllerId, KeymapInput input, KeymapOutConfig config)
        {
            string inputKey = input.Key;
            string screen = "OnScreen";
            string[] parts = inputKey.Split('.');
            if (parts[0] == "OffScreen")
            {
                screen = parts[0];
                inputKey = string.Join(".", parts.Skip(1));
            }

            string key;
            if (controllerId == 0)
            {
                key = "All";
            }
            else
            {
                key = "" + controllerId;
            }

            {
                JToken level1 = this.jsonObj.GetValue(key);
                if (level1 == null || level1.Type != JTokenType.Object)
                {
                    jsonObj.Add(key, new JObject());
                }
                level1 = this.jsonObj.GetValue(key);

                JToken level2 = ((JObject)level1).GetValue(screen);
                if (level2 == null || level2.Type != JTokenType.Object)
                {
                    ((JObject)level1).Add(screen, new JObject());
                }
                level2 = ((JObject)level1).GetValue(screen);

                JToken level3 = ((JObject)level2).GetValue(inputKey);
                if (level3 != null)
                {
                    ((JObject)level2).Remove(inputKey);
                }

                if (!config.Inherited)
                {
                    JToken outputs = null;

                    if (config.Stack.Count > 1)
                    {
                        JArray array = new JArray();
                        foreach (KeymapOutput output in config.Stack)
                        {
                            array.Add(output.Key);
                        }
                        outputs = array;
                    }
                    else if (config.Stack.Count == 1)
                    {
                        outputs = config.Stack.First().Key;
                    }

                    if (config.Scale != Settings.Default.defaultContinousScale || config.Threshold != Settings.Default.defaultContinousPressThreshold || config.Deadzone != Settings.Default.defaultContinousDeadzone)
                    {
                        JObject settings = new JObject();
                        if (config.Scale != Settings.Default.defaultContinousScale)
                        {
                            settings.Add("scale", config.Scale);
                        }
                        if (config.Threshold != Settings.Default.defaultContinousPressThreshold)
                        {
                            settings.Add("threshold", config.Threshold);
                        }
                        if (config.Deadzone != Settings.Default.defaultContinousDeadzone)
                        {
                            settings.Add("deadzone", config.Deadzone);
                        }
                        settings.Add("output", outputs);
                        outputs = settings;
                    }
                    ((JObject)level2).Add(inputKey, outputs);
                }
                jsonObj.Remove(key);
                jsonObj.Add(key, level1);
            }
            save();
        }

        //0 = all
        public KeymapOutConfig getConfigFor(int controllerId, string input)
        {
            string inputKey = input;
            string screen = "OnScreen";
            string[] parts = inputKey.Split('.');

            if (parts[0] == "OffScreen")
            {
                screen = parts[0];
                inputKey = string.Join(".", parts.Skip(1));
            }

            string key;
            if (controllerId > 0)
            {
                key = "" + controllerId;
            }
            else
            {
                key = "All";
            }

            JToken level1 = this.jsonObj.GetValue(key);
            if (level1 != null && level1.Type == JTokenType.Object)
            {
                JToken level2 = ((JObject)level1).GetValue(screen);
                if (level2 != null && level2.Type == JTokenType.Object)
                {
                    JToken level3 = ((JObject)level2).GetValue(inputKey);
                    if (level3 != null)
                    {
                        KeymapOutConfig outconfig = null;
                        if (level3.Type == JTokenType.String)
                        {
                            if (KeymapDatabase.Current.getOutput(level3.ToString().ToLower()) != null)
                            {
                                outconfig = new KeymapOutConfig(KeymapDatabase.Current.getOutput(level3.ToString().ToLower()), false);
                            }
                        }
                        else if (level3.Type == JTokenType.Array)
                        {
                            JArray array = (JArray)level3;
                            List<KeymapOutput> result = new List<KeymapOutput>();
                            foreach (JValue value in array)
                            {
                                if (KeymapDatabase.Current.getOutput(value.ToString().ToLower()) != null)
                                {
                                    result.Add(KeymapDatabase.Current.getOutput(value.ToString().ToLower()));
                                }
                            }
                            if (result.Count == array.Count)
                            {
                                outconfig = new KeymapOutConfig(result, false);
                            }
                        }
                        else if (level3.Type == JTokenType.Object)
                        {
                            JToken level4 = ((JObject)level3).GetValue("output");
                            if (level4 != null)
                            {
                                if (level4.Type == JTokenType.String)
                                {
                                    if (KeymapDatabase.Current.getOutput(level4.ToString().ToLower()) != null)
                                    {
                                        outconfig = new KeymapOutConfig(KeymapDatabase.Current.getOutput(level4.ToString().ToLower()), false);
                                    }
                                }
                                else if (level4.Type == JTokenType.Array)
                                {
                                    JArray array = (JArray)level4;
                                    List<KeymapOutput> result = new List<KeymapOutput>();
                                    foreach (JValue value in array)
                                    {
                                        if (KeymapDatabase.Current.getOutput(value.ToString().ToLower()) != null)
                                        {
                                            result.Add(KeymapDatabase.Current.getOutput(value.ToString().ToLower()));
                                        }
                                    }
                                    if (result.Count == array.Count)
                                    {
                                        outconfig = new KeymapOutConfig(result, false);
                                    }
                                }

                                if (outconfig != null)
                                {
                                    if (((JObject)level3).GetValue("scale") != null && ((JObject)level3).GetValue("scale").Type == JTokenType.Float)
                                    {
                                        outconfig.Scale = Double.Parse(((JObject)level3).GetValue("scale").ToString());
                                    }

                                    if (((JObject)level3).GetValue("threshold") != null && ((JObject)level3).GetValue("threshold").Type == JTokenType.Float)
                                    {
                                        outconfig.Threshold = Double.Parse(((JObject)level3).GetValue("threshold").ToString());
                                    }

                                    if (((JObject)level3).GetValue("deadzone") != null && ((JObject)level3).GetValue("deadzone").Type == JTokenType.Float)
                                    {
                                        outconfig.Deadzone = Double.Parse(((JObject)level3).GetValue("deadzone").ToString());
                                    }
                                }
                            }
                        }
                        return outconfig;
                    }
                }
            }
            //Try to inherit from OffScreen key in the "All" setting
            if (screen == "OffScreen" && controllerId > 0)
            {
                KeymapOutConfig result = this.getConfigFor(0, screen + "." + inputKey);
                if (result != null && !result.Inherited)
                {
                    return result;
                }
            }
            //If not found inherit from OnScreen setting for current controller
            if (screen == "OffScreen")
            {
                KeymapOutConfig result = this.getConfigFor(controllerId, inputKey);
                if (inputKey == "Pointer")
                {
                    result = new KeymapOutConfig(KeymapDatabase.Current.getDisableOutput(), false);
                    result.Inherited = true;
                    return result;
                }
                if (result != null)
                {
                    result.Inherited = true;
                }
                return result;
            }
            if (controllerId > 0)
            {
                //If we are searching for controller-specific keymaps we can inherit from the "All" setting.
                KeymapOutConfig result = this.getConfigFor(0, inputKey);
                if (result != null)
                {
                    result.Inherited = true;
                }
                return result;
            }
            //If we can not find any setting in the All group, search for inherit from the default keymap
            if (this.Parent != null)
            {
                KeymapOutConfig result = this.Parent.getConfigFor(controllerId, inputKey);
                if (result != null)
                {
                    result.Inherited = true;
                }
                return result;
            }
            else
            {
                //This means we have no setting for the input on this keymap nor any keymap to inherit from. Let's save a "Disable" setting on this.
                this.setConfigFor(controllerId, KeymapDatabase.Current.getInput(input), new KeymapOutConfig(KeymapDatabase.Current.getDisableOutput(), false));
                //It's a small chance of deadlock here if the above command doesnt work, but wth
                return this.getConfigFor(controllerId, inputKey);
            }
        }

    }

    public class KeymapOutConfig
    {
        public bool Inherited;
        public double Scale = Settings.Default.defaultContinousScale;
        public double Threshold = Settings.Default.defaultContinousPressThreshold;
        public double Deadzone = Settings.Default.defaultContinousDeadzone;
        public List<KeymapOutput> Stack;

        public KeymapOutConfig(KeymapOutput output, bool inherited)
        {
            this.Stack = new List<KeymapOutput>();
            this.Stack.Add(output);
            this.Inherited = inherited;
        }

        public KeymapOutConfig(List<KeymapOutput> output, bool inherited)
        {
            this.Stack = output;
            this.Inherited = inherited;
        }

        public void addOutput(KeymapOutput output)
        {
            this.Stack.Add(output);
        }
    }
}

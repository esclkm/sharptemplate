using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
/*
 * Шаблонизатор на C# 
 * идея шаблонизатора взята из Cotonti
 * переменные в шаблоне выглядят как %_VAR_%
 * Блоки как [[ BEGIN: BLOCKNAME ]][[ END: BLOCKNAME ]]
 * Позиция блока: [[_BLOCKNAME_]]
 * 
 * */
namespace CDT
{
    class xtemplate
    {
        Dictionary<string, string> template_variables; //
        Dictionary<string, string> template_content;
        Dictionary<string, string> template_modified;
        Dictionary<string, string> template_final;

        /* Шаблоны строк */
        string t_block = @"\[\[ BEGIN: ([A-Z0-9\._]+) \]\]\r*\n*([\s\S]+)?\[\[ END: \1 \]\]\r*\n*";
        string t_block_format = @"[[%{0}%]]";
        string t_block_begin = @"\[\[ BEGIN: ([A-Z0-9\._]+) \]\]\r*\n*";
        string t_block_end = @"\[\[ END: ([A-Z0-9\._]+) \]\]\r*\n*";
        string t_var_block = @"\[\[%([A-Z0-9\._]+)%\]\]";

        string t_var = @"\%%([A-Z0-9\._]+)%\%";
        string t_var_format = "%%{0}%%";

        string t_if = @"\[\[ IF: (.*) \]\]\r*\n*([\s\S]+?)\[\[ ENDIF \]\]\r*\n*";
        string t_if_format = @"\[\[ IF: {0} \]\]\r*\n*([\s\S]+?)\[\[ ENDIF \]\]\r*\n*";
        string t_if_begin = @"\[\[ IF: (.*) \]\]\r*\n*";
        string t_if_end = @"\[\[ ENDIF \]\]\r*\n*";

        public xtemplate(string path)
        {
            template_variables = new Dictionary<string, string>();
            template_content = new Dictionary<string, string>();
            template_modified = new Dictionary<string, string>();
            template_final = new Dictionary<string, string>();

            if (System.IO.File.Exists(path))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(path))
                    {
                        string text = sr.ReadToEnd();
                        text = Regex.Replace(text, @"^[\t ]+\[\[ ", @"[[ ", RegexOptions.Multiline);
                        getblocks(text, "");
                    }

                }
                catch (Exception e2)
                {
                    // Let the user know what went wrong.
                    MessageBox.Show(e2.Message);
                }
            }
       
        }
        /*
         * Суть данного метода получить список блоков в данном тесте, занести их в массив
         * если внутри есть еще блоки, то перезапустить конструкцию
         * вместо блоков создать соответствующие переменные
         */
        private void getblocks(string text, string block)
        {
            MatchCollection matches = Regex.Matches(text, t_block, RegexOptions.Multiline);
            
            block = (block != "") ? block + "." : "";

            foreach (Match match in matches)
            {
                string stext = Regex.Replace(match.Groups[2].Value, t_block, String.Format(t_block_format, block + match.Groups[1].Value + ".$1"));
                try
                {
                    template_content.Add(block + match.Groups[1].Value, stext);
                }
                catch { }
                getblocks(match.Groups[2].Value, block + match.Groups[1].Value);
            }
        }
        /*
         * метод добавляет ключ значение
         */
        public void assign(string key, string value)
        {
            template_variables[key] = value;
        }
        /*
         * Пасим необходимый блок
         */
        public void parse(string block)
        {

            
            if(template_content.ContainsKey(block))
            {

                string content = (template_modified.ContainsKey(block)) ? template_modified[block] : template_content[block];
                content = parsevariables(content);
                content = parseif(content);
                content = cleanblock(content);

                if (block.IndexOf(".") > -1)
                {
                    string parent_block = Regex.Replace(block, @"(.+)\.(.+)?", "$1");
                    string parent_content = (template_modified.ContainsKey(parent_block)) ? template_modified[parent_block] : template_content[parent_block];

                    parent_content = parent_content.Replace(String.Format(t_block_format, block), content + String.Format(t_block_format, block));
                    template_modified[parent_block] = parent_content;

                }
                template_final[block] = content;

                if(template_modified.ContainsKey(block))
                {
                    template_modified.Remove(block);
                }
            }

        }
        /* Парсим переменные в блоке */
        private string parsevariables(string content)
        {
            MatchCollection matches = Regex.Matches(content, t_var, RegexOptions.Multiline);
            foreach (Match match in matches)
            {
                string var = (template_variables.ContainsKey(match.Groups[1].Value)) ? template_variables[match.Groups[1].Value] : "";
                content = content.Replace(String.Format(t_var_format, match.Groups[1].Value), var);
            }
            return content;
        }
        /* парсим ифы : достуные варианты !, =, !=, > , <*/ 
        private string parseif(string content)
        {
            string condpattern = @"(.*) (=|\!=|>|<) (.*)";

            MatchCollection matches = Regex.Matches(content, t_if, RegexOptions.Multiline);

            foreach (Match match in matches)
            {
                string condition = match.Groups[1].Value;
                Match mt = Regex.Match(condition, condpattern, RegexOptions.IgnoreCase);
                bool parseif = false;
                
                if (mt.Success)
                {
                    // Finally, we get the Group value and display it.
                    if (mt.Groups[2].Value == "=" && mt.Groups[1].Value == mt.Groups[3].Value)
                    {
                        parseif = true;
                    }
                    if(mt.Groups[2].Value == "!=" && mt.Groups[1].Value != mt.Groups[3].Value)
                    {
                        parseif = true;
                    }
                    if (!parseif)
                    {
                        int int1, int3;
                        bool parsed1 = Int32.TryParse(mt.Groups[1].Value, out int1);
                        bool parsed3 = Int32.TryParse(mt.Groups[3].Value, out int3);
                        if (parsed1 && parsed3)
                        {
                            if (mt.Groups[2].Value == ">" && int1 > int3)
                            {
                                parseif = true;
                            }
                            if (mt.Groups[2].Value == "<" && int1 < int3)
                            {
                                parseif = true;
                            }
                        }
                    }
                }
                else
                {

                    if (condition!="" && condition.Substring(0, 1) == "!")
                    {
                        if (condition.Length == 0 || condition == "0")
                        {
                            parseif = true;
                        }
                    }
                    else
                    {
                        if(condition.Length > 0 && condition != "0")
                        {
                            parseif = true;
                        }
                    }
                }
                // Вот тут выходит фигня надо экранироавть символы
                condition= condition.Replace("!", @"\!").Replace(".", @"\.");
                content = Regex.Replace(content, String.Format(t_if_format, condition), (parseif) ? "$1" : "", RegexOptions.Multiline);

            }
            return content;
        }
        /* чистим случайный мусор */
        private string cleanblock(string content)
        {
            content = Regex.Replace(content, t_var, "",  RegexOptions.Multiline);
            content = Regex.Replace(content, t_var_block, "", RegexOptions.Multiline);

            content = Regex.Replace(content, t_if, "", RegexOptions.Multiline);
            content = Regex.Replace(content, t_if_begin, "", RegexOptions.Multiline);
            content = Regex.Replace(content, t_if_end, "", RegexOptions.Multiline);

            content = Regex.Replace(content, t_block, "", RegexOptions.Multiline);
            content = Regex.Replace(content, t_block_begin, "", RegexOptions.Multiline);
            content = Regex.Replace(content, t_block_end, "", RegexOptions.Multiline);
            return content;
        }
        /* вывод текста из блока, если блок спаршен */
        public string text(string block)
        {
            if(!template_final.ContainsKey(block))
            {
                return "";
            }
            return template_final[block];
        }
        /* сохраняем блок в файл */
        public void save(string block, string file)
        {
            string text = (!template_final.ContainsKey(block)) ? "" : template_final[block];
            using (StreamWriter outfile = new StreamWriter(file, false, new System.Text.UTF8Encoding(false)))
            {
                outfile.Write(text);
            }
        }
        public void alert()
        {

            foreach (KeyValuePair<string, string> kvp in template_content)
            {
                MessageBox.Show("Key = " + kvp.Key + ", Value = " + kvp.Value);
            }
        }

    }
}

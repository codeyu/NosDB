﻿// /*
// * Copyright (c) 2016, Alachisoft. All Rights Reserved.
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// * http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Alachisoft.NosDB.Common.Configuration;
namespace Alachisoft.NosDB.Common.Tools
{
    public class CommandLineArgumentParser
    {
        public static void CommandLineParser(ref object obj, string[] args)
        {
            Type type;
            PropertyInfo[] objProps;
            ConfigurationBuilder configBuilder = new ConfigurationBuilder();
            type = obj.GetType();
            objProps = type.GetProperties();
            ArgumentAttribute orphanAttribute = null;
            PropertyInfo orphanPropInfo = null;


            if (objProps != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    PropertyInfo propInfo;
                    Object[] customParams;
                    int propLoc = i;
                    bool isAssigned = false;
                    for (int j = 0; j < objProps.Length; j++)
                    {
                        propInfo = objProps[j];
                        customParams = propInfo.GetCustomAttributes(typeof(ArgumentAttribute), false);


                        if (customParams != null && customParams.Length > 0)
                        {
                            ArgumentAttribute param = customParams[0] as ArgumentAttribute;
                            try
                            {
                                if (param != null && (param.ShortNotation == args[i] || param.FullName.ToLower() == args[i].ToLower()))
                                {

                                    if (propInfo.PropertyType.FullName == "System.Boolean")
                                    {
                                        bool value = (bool)param.DefaultValue;

                                        if (value)
                                            propInfo.SetValue(obj, false, null);
                                        else
                                            propInfo.SetValue(obj, true, null);
                                        isAssigned = true;
                                        break;
                                    }
                                    else
                                    {
                                        int index = i + 1;
                                        if (index <= (args.Length - 1))
                                        {
                                            object value = configBuilder.ConvertToPrimitive(propInfo.PropertyType, args[++i], null);
                                            if (propInfo.PropertyType.IsAssignableFrom(value.GetType()))
                                            {
                                                propInfo.SetValue(obj, value, null);
                                                isAssigned = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else if (param != null && (string.IsNullOrEmpty(param.ShortNotation) && string.IsNullOrEmpty(param.FullName)))
                                {
                                    if (orphanAttribute == null && !isAssigned)
                                    {
                                        orphanAttribute = param;
                                        orphanPropInfo = propInfo;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                throw new Exception("Can not set the value  " + param.ShortNotation + " Error :" + e.Message.ToString());
                            }

                        }
                    }
                    if (!isAssigned)
                    {
                        if (orphanAttribute != null && orphanPropInfo != null)
                        {
                            if (orphanPropInfo.GetValue(obj, null) != null)
                            {
                                object value = configBuilder.ConvertToPrimitive(orphanPropInfo.PropertyType, args[i], null);
                                if (orphanPropInfo.PropertyType.IsAssignableFrom(value.GetType()))
                                {
                                    orphanPropInfo.SetValue(obj, value, null);
                                }
                            }
                        }
                    }
                }
            }
        }



    }
}
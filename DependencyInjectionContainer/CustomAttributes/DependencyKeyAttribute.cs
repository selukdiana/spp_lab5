﻿using System;

namespace DependencyInjectionContainer.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class DependencyKeyAttribute : System.Attribute
    {
        public object Key { get; }

        public DependencyKeyAttribute(object key)
        {
            Key = key;
        }
    }
}

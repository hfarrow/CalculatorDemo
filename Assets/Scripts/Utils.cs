using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CalculatorDemo
{
    public static class Utils
    {
		public static MethodInfo GetMathFunction(string functionName, int argCount)
		{
			MethodInfo methodInfo = typeof(Math)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.FirstOrDefault(mi =>
					mi.Name.Equals(functionName, StringComparison.OrdinalIgnoreCase) &&
					mi.GetParameters().Length == argCount &&
					mi.GetParameters().All(p => p.ParameterType == typeof(double)));

			return methodInfo;
		}

	    public static Transform Search(this Transform target, string name)
	    {
		    if (target.name == name) return target;
 
		    for (int i = 0; i < target.childCount; ++i)
		    {
			    Transform result = Search(target.GetChild(i), name);
             
			    if (result != null) return result;
		    }
 
		    return null;
	    }
    }
}
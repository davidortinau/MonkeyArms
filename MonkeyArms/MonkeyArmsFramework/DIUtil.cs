using System;
using System.Reflection;

namespace MonkeyArms
{
	public class InjectAttribute : System.Attribute { }

	public class DIUtil
	{
		public DIUtil ()
		{
		}

		public static void InjectProps (IInjectingTarget target)
		{
			Inject (target.GetType ().GetProperties (), target);
			Inject (target.GetType ().GetFields (), target);

		}

		static void Inject (MemberInfo[] memberInfoArray, IInjectingTarget target)
		{
			foreach (var memberInfo in memberInfoArray) {

				object[] attrs = GetAttributes (target, memberInfo);

				foreach (Attribute attr in attrs) {


					if (attr is InjectAttribute) {


						//retrieve injectable from DI
						var mi = typeof(DI).GetMethod ("Get", BindingFlags.Static | BindingFlags.Public);

						//getting method to get value to inject
						MethodInfo methodInfo = GetMethodInfo (mi, memberInfo);


						
						//Checking if value was found
						var valueToInject = methodInfo.Invoke (null, null);
						if (valueToInject == null) {
							throw(new ArgumentException ("Inject target type was not found. Did you forget to register it with DI?"));
						} else {
							AssignValueToTarget (target, memberInfo, valueToInject);
						}
					}
				}
			}
		}

		static void AssignValueToTarget (IInjectingTarget target, MemberInfo memberInfo, object valueToInject)
		{
			if (memberInfo is System.Reflection.FieldInfo) {
				(memberInfo as System.Reflection.FieldInfo).SetValue (target, valueToInject);
			}
			if (memberInfo is System.Reflection.PropertyInfo) {
				(memberInfo as System.Reflection.PropertyInfo).SetValue (target, valueToInject, null);
			}
		}

		static MethodInfo GetMethodInfo (MethodInfo mi, MemberInfo memberInfo)
		{

			if (memberInfo is FieldInfo) {
				return mi.MakeGenericMethod ((memberInfo as FieldInfo).FieldType);
			}
			if (memberInfo is PropertyInfo) {
				return mi.MakeGenericMethod ((memberInfo as PropertyInfo).PropertyType);
			}
			return null;
		}

		static object[] GetAttributes (IInjectingTarget target, System.Reflection.MemberInfo memberInfo)
		{
			if (memberInfo is FieldInfo) {
				return target.GetType ().GetField (memberInfo.Name).GetCustomAttributes (false);
			}
			if (memberInfo is PropertyInfo) {
				return target.GetType ().GetProperty (memberInfo.Name).GetCustomAttributes (false);
			}
			throw(new ArgumentException ("Inject tag was places on a member type injection doesn't support"));
		}
	}

	public interface IInjectingTarget
	{
		
	}
}


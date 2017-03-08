using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Primitives;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace AspNetCoreTmp
{
    public class AspNetDiagnosticListener
    {
		private static readonly DiagnosticListener _httpListener = new DiagnosticListener("Microsoft.AspNetCore");
		private const string ActivityName = "Microsoft.AspNetCore.Activity";
		private const string ActivityStartName = "Microsoft.AspNetCore.Activity.Start";
		private static readonly PropertyFetcher contextFetcher = new PropertyFetcher("httpContext");
		public static IDisposable Enable()
		{
			return DiagnosticListener.AllListeners.Subscribe(delegate (DiagnosticListener listener) {
				if (listener.Name == "Microsoft.AspNetCore")
				{
					listener.Subscribe(delegate (KeyValuePair<string, object> value)
					{
						if (value.Key == "Microsoft.AspNetCore.Hosting.BeginRequest")
						{
							var context = (HttpContext)contextFetcher.Fetch(value.Value);
							if (_httpListener.IsEnabled(ActivityName))
							{
								Activity activity = new Activity(ActivityName);

								//add tags, baggage, etc.
								StringValues requestId;
								if (context.Request.Headers.TryGetValue("Request-Id", out requestId))
								{
									activity.SetParentId(requestId.First());
                                    string[] baggage = context.Request.Headers.GetCommaSeparatedValues("Correlation-Context");
                                    if (baggage != null)
									{
										foreach (var item in baggage)
										{
											NameValueHeaderValue baggageItem;
											if (NameValueHeaderValue.TryParse(item, out baggageItem))
											{
												activity.AddBaggage(baggageItem.Name, baggageItem.Value);
											}
										}
									}
								}


								//before starting an activity, check that user wants this request to be instumented
								if (_httpListener.IsEnabled(ActivityName, activity, context))
								{
									if (_httpListener.IsEnabled(ActivityStartName)) //allow Stop events only to reduce verbosity, but start activity anyway
									{
										_httpListener.StartActivity(activity, new { httpContext = context, timestamp = Stopwatch.GetTimestamp() });
									}
									else
									{
										activity.Start();
									}
								}
							}
						}
						else if (value.Key == "Microsoft.AspNetCore.Hosting.EndRequest")
						{
							var context = (HttpContext)contextFetcher.Fetch(value.Value);
							var rootActivity = Activity.Current;
							if (rootActivity != null)
							{
								while (rootActivity.Parent != null)
								{
									rootActivity = rootActivity.Parent;
								}
								rootActivity.SetEndTime(DateTime.UtcNow);
								_httpListener.StopActivity(rootActivity, new { httpContext = context, timestamp = Stopwatch.GetTimestamp() });
							}
						}
					});
				}
			});
		}

		#region private

		private class PropertyFetcher
		{
			public PropertyFetcher(string propertyName)
			{
				this.propertyName = propertyName;
			}

			public object Fetch(object obj)
			{
				if (innerFetcher == null)
				{
					innerFetcher = PropertyFetch.FetcherForProperty(obj.GetType().GetTypeInfo().GetDeclaredProperty(propertyName));
				}

				return innerFetcher?.Fetch(obj);
			}

			#region private

			private PropertyFetch innerFetcher;
			private readonly string propertyName;

			//see https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.DiagnosticSource/src/System/Diagnostics/DiagnosticSourceEventSource.cs
			class PropertyFetch
			{
				/// <summary>
				/// Create a property fetcher from a .NET Reflection PropertyInfo class that
				/// represents a property of a particular type.  
				/// </summary>
				public static PropertyFetch FetcherForProperty(PropertyInfo propertyInfo)
				{
					if (propertyInfo == null)
						return new PropertyFetch(); // returns null on any fetch.

					var typedPropertyFetcher = typeof(TypedFetchProperty<,>);
					var instantiatedTypedPropertyFetcher = typedPropertyFetcher.GetTypeInfo().MakeGenericType(
						propertyInfo.DeclaringType, propertyInfo.PropertyType);
					return (PropertyFetch)Activator.CreateInstance(instantiatedTypedPropertyFetcher, propertyInfo);
				}

				/// <summary>
				/// Given an object, fetch the property that this propertyFech represents. 
				/// </summary>
				public virtual object Fetch(object obj)
				{
					return null;
				}

				#region private 

				private class TypedFetchProperty<TObject, TProperty> : PropertyFetch
				{
					public TypedFetchProperty(PropertyInfo property)
					{
						_propertyFetch =
							(Func<TObject, TProperty>)
							property.GetMethod.CreateDelegate(typeof(Func<TObject, TProperty>));
					}

					public override object Fetch(object obj)
					{
						return _propertyFetch((TObject)obj);
					}

					private readonly Func<TObject, TProperty> _propertyFetch;
				}

				#endregion
			}

			#endregion
		}
		#endregion
	}

}

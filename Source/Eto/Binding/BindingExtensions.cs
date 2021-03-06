using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Eto
{
	/// <summary>
	/// Extensions for bindings
	/// </summary>
	public static class BindingExtensions
	{
		/// <summary>
		/// Adds a new dual binding between the widget and the specified object
		/// </summary>
		/// <param name="widget">Widget to add the binding to</param>
		/// <param name="widgetPropertyName">Property on the widget to update</param>
		/// <param name="source">Object to bind to</param>
		/// <param name="sourcePropertyName">Property on the source object to retrieve/set the value of</param>
		/// <param name="mode">Mode of the binding</param>
		/// <returns>A new instance of the DualBinding class that is used to control the binding</returns>
		public static DualBinding Bind(this Widget widget, string widgetPropertyName, object source, string sourcePropertyName, DualBindingMode mode = DualBindingMode.TwoWay)
		{
			var binding = new DualBinding(
				source,
				sourcePropertyName,
				widget,
				widgetPropertyName,
				mode
			);
			widget.Bindings.Add(binding);
			return binding;
		}
		
		/// <summary>
		/// Adds a new dual binding between the widget and the specified source binding
		/// </summary>
		/// <param name="widget">Widget to add the binding to</param>
		/// <param name="widgetPropertyName">Property on the widget to update</param>
		/// <param name="sourceBinding">Binding to get/set the value to from the widget</param>
		/// <param name="mode">Mode of the binding</param>
		/// <returns>A new instance of the DualBinding class that is used to control the binding</returns>
		public static DualBinding Bind(this Widget widget, string widgetPropertyName, DirectBinding sourceBinding, DualBindingMode mode = DualBindingMode.TwoWay)
		{
			var binding = new DualBinding(
				sourceBinding,
				new ObjectBinding(widget, widgetPropertyName),
				mode
			);
			widget.Bindings.Add(binding);
			return binding;
		}
		
		/// <summary>
		/// Adds a new binding with the widget and the the widget's current data context 
		/// </summary>
		/// <remarks>
		/// This binds to a property of the <see cref="InstanceWidget.DataContext"/>, which will return the topmost value
		/// up the control hierarchy.  For example, you can set the DataContext of your form or panel, and then bind to properties
		/// of that context on any of the child controls such as a text box, etc.
		/// </remarks>
		/// <param name="widget">Widget to add the binding to</param>
		/// <param name="widgetPropertyName">Property on the widget to update</param>
		/// <param name="dataContextPropertyName">Property on the widget's <see cref="InstanceWidget.DataContext"/> to bind to the widget</param>
		/// <param name="mode">Mode of the binding</param>
		/// <param name="defaultWidgetValue">Default value to set to the widget when the value from the DataContext is null</param>
		/// <param name="defaultContextValue">Default value to set to the DataContext property when the widget value is null</param>
		/// <returns>A new instance of the DualBinding class that is used to control the binding</returns>
		public static DualBinding Bind(this InstanceWidget widget, string widgetPropertyName, string dataContextPropertyName, DualBindingMode mode = DualBindingMode.TwoWay, object defaultWidgetValue = null, object defaultContextValue = null)
		{
			var dataContextBinding = new PropertyBinding(dataContextPropertyName);
			var widgetBinding = new PropertyBinding(widgetPropertyName);
			return Bind(widget, widgetBinding, dataContextBinding, mode, defaultWidgetValue, defaultContextValue);
		}

		public static DualBinding Bind<W,WP,S,SP>(this W widget, Expression<Func<W,WP>> widgetProperty, S source, Expression<Func<S, SP>> sourceProperty, DualBindingMode mode = DualBindingMode.TwoWay)
			where W: InstanceWidget
		{
			var widgetExpression = (MemberExpression)widgetProperty.Body;
			var sourceExpression = (MemberExpression)sourceProperty.Body;
			return Bind(widget, widgetExpression.Member.Name, source, sourceExpression.Member.Name, mode);
		}

		public static DualBinding Bind<W, WP, SP, DC>(this W widget, Expression<Func<W, WP>> widgetProperty, Expression<Func<DC, SP>> sourceProperty, DualBindingMode mode = DualBindingMode.TwoWay, object defaultWidgetValue = null, object defaultContextValue = null)
			where W : InstanceWidget
		{
			var widgetExpression = (MemberExpression)widgetProperty.Body;
			var sourceExpression = (MemberExpression)sourceProperty.Body;
			return Bind(widget, widgetExpression.Member.Name, sourceExpression.Member.Name, mode, defaultWidgetValue, defaultContextValue);
		}

		public static DualBinding Bind(this InstanceWidget widget, IndirectBinding widgetBinding, DirectBinding valueBinding, DualBindingMode mode = DualBindingMode.TwoWay)
		{
			return Bind(widgetBinding: new ObjectBinding(widget, widgetBinding), valueBinding: valueBinding, mode: mode);
		}

		public static DualBinding Bind(this InstanceWidget widget, IndirectBinding widgetBinding, object objectValue, IndirectBinding objectBinding, DualBindingMode mode = DualBindingMode.TwoWay, object defaultWidgetValue = null, object defaultContextValue = null)
		{
			var valueBinding = new ObjectBinding(objectValue, objectBinding) {
				SettingNullValue = defaultContextValue,
				GettingNullValue = defaultWidgetValue
			};
			return Bind(widget, widgetBinding, valueBinding, mode);
		}

		public static DualBinding Bind(this InstanceWidget widget, IndirectBinding widgetBinding, IndirectBinding dataContextBinding, DualBindingMode mode = DualBindingMode.TwoWay, object defaultWidgetValue = null, object defaultContextValue = null)
		{
			return Bind(new ObjectBinding(widget, widgetBinding), dataContextBinding, mode, defaultWidgetValue, defaultContextValue);
		}

		public static DualBinding Bind(this ObjectBinding widgetBinding, DirectBinding valueBinding, DualBindingMode mode = DualBindingMode.TwoWay)
		{
			var binding = new DualBinding(
				valueBinding,
				widgetBinding,
				mode
			);
			var widget = widgetBinding.DataItem as InstanceWidget;
			if (widget != null)
				widget.Bindings.Add(binding);
			return binding;
		}

		public static DualBinding Bind(this ObjectBinding widgetBinding, IndirectBinding dataContextBinding, DualBindingMode mode = DualBindingMode.TwoWay, object defaultWidgetValue = null, object defaultContextValue = null)
		{
			var widget = widgetBinding.DataItem as InstanceWidget;
			if (widget == null)
				throw new ArgumentOutOfRangeException("widgetBinding", "Binding must be attached to a widget");
			var contextBinding = new ObjectBinding(widget, new DelegateBinding<InstanceWidget, object>(w => w.DataContext, null, (w, h) => w.DataContextChanged += h, (w, h) => w.DataContextChanged -= h));
			var valueBinding = new ObjectBinding(widget.DataContext, dataContextBinding) {
				GettingNullValue = defaultWidgetValue,
				SettingNullValue = defaultContextValue
			};
			DualBinding binding = Bind(widgetBinding: widgetBinding, valueBinding: valueBinding, mode: mode);
			contextBinding.DataValueChanged += delegate
			{
				((ObjectBinding)binding.Source).DataItem = contextBinding.DataValue;
			};
			widget.Bindings.Add(contextBinding);
			return binding;
		}
	}
}
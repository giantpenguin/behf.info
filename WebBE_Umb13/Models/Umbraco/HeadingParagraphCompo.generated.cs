//------------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//
//    Umbraco.ModelsBuilder.Embedded v13.5.2+3431f76
//
//   Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.PublishedModels
{
	// Mixin Content Type with alias "headingParagraphCompo"
	/// <summary>Heading Paragraph Component</summary>
	public partial interface IHeadingParagraphCompo : IPublishedElement
	{
		/// <summary>Heading</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.5.2+3431f76")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		string Heading { get; }

		/// <summary>Paragraph</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.5.2+3431f76")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		global::Umbraco.Cms.Core.Strings.IHtmlEncodedString Paragraph { get; }
	}

	/// <summary>Heading Paragraph Component</summary>
	[PublishedModel("headingParagraphCompo")]
	public partial class HeadingParagraphCompo : PublishedElementModel, IHeadingParagraphCompo
	{
		// helpers
#pragma warning disable 0109 // new is redundant
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.5.2+3431f76")]
		public new const string ModelTypeAlias = "headingParagraphCompo";
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.5.2+3431f76")]
		public new const PublishedItemType ModelItemType = PublishedItemType.Content;
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.5.2+3431f76")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public new static IPublishedContentType GetModelContentType(IPublishedSnapshotAccessor publishedSnapshotAccessor)
			=> PublishedModelUtility.GetModelContentType(publishedSnapshotAccessor, ModelItemType, ModelTypeAlias);
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.5.2+3431f76")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public static IPublishedPropertyType GetModelPropertyType<TValue>(IPublishedSnapshotAccessor publishedSnapshotAccessor, Expression<Func<HeadingParagraphCompo, TValue>> selector)
			=> PublishedModelUtility.GetModelPropertyType(GetModelContentType(publishedSnapshotAccessor), selector);
#pragma warning restore 0109

		private IPublishedValueFallback _publishedValueFallback;

		// ctor
		public HeadingParagraphCompo(IPublishedElement content, IPublishedValueFallback publishedValueFallback)
			: base(content, publishedValueFallback)
		{
			_publishedValueFallback = publishedValueFallback;
		}

		// properties

		///<summary>
		/// Heading
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.5.2+3431f76")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		[ImplementPropertyType("heading")]
		public virtual string Heading => GetHeading(this, _publishedValueFallback);

		/// <summary>Static getter for Heading</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.5.2+3431f76")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public static string GetHeading(IHeadingParagraphCompo that, IPublishedValueFallback publishedValueFallback) => that.Value<string>(publishedValueFallback, "heading");

		///<summary>
		/// Paragraph
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.5.2+3431f76")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		[ImplementPropertyType("paragraph")]
		public virtual global::Umbraco.Cms.Core.Strings.IHtmlEncodedString Paragraph => GetParagraph(this, _publishedValueFallback);

		/// <summary>Static getter for Paragraph</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.5.2+3431f76")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public static global::Umbraco.Cms.Core.Strings.IHtmlEncodedString GetParagraph(IHeadingParagraphCompo that, IPublishedValueFallback publishedValueFallback) => that.Value<global::Umbraco.Cms.Core.Strings.IHtmlEncodedString>(publishedValueFallback, "paragraph");
	}
}

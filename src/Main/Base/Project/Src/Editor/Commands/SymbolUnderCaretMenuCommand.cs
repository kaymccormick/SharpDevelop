﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using ICSharpCode.Core;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Parser;

namespace ICSharpCode.SharpDevelop.Editor.Commands
{
	/// <summary>
	/// A menu command that operates on a <see cref="ResolveResult"/>.
	/// 
	/// Supports the following types as <see cref="Owner"/>:
	/// - ResolveResult
	/// - IEntityModel
	/// 
	/// If the owner isn't one of the types above, the command operates on the caret position in the current editor.
	/// </summary>
	public abstract class ResolveResultMenuCommand : AbstractMenuCommand
	{
		public abstract void Run(ResolveResult symbol);
		
		public override void Run()
		{
			ITextEditor editor = SD.GetActiveViewContentService<ITextEditor>();
			ResolveResult resolveResult = GetResolveResult(editor, Owner);
			Run(resolveResult);
		}
		
		public static ResolveResult GetResolveResult(object owner)
		{
			return GetResolveResult(null, owner);
		}
		
		public static IEntity GetEntity(object owner)
		{
			return GetEntity(GetResolveResult(null, owner));
		}
		
		static ResolveResult GetResolveResult(ITextEditor currentEditor, object owner)
		{
			if (owner is ResolveResult) {
				return (ResolveResult)owner;
			} else if (owner is IEntityModel) {
				return GetResolveResultFromEntityModel((IEntityModel)owner);
			} else if (currentEditor != null) {
				return SD.ParserService.Resolve(currentEditor, currentEditor.Caret.Location);
			} else {
				return ErrorResolveResult.UnknownError;
			}
		}
		
		static ResolveResult GetResolveResultFromEntityModel(IEntityModel entityModel)
		{
			IEntity entity = entityModel.Resolve();
			if (entity is IMember)
				return new MemberResolveResult(null, (IMember)entity);
			if (entity is ITypeDefinition)
				return new TypeResolveResult((ITypeDefinition)entity);
			return ErrorResolveResult.UnknownError;
		}
		
		protected static IEntity GetEntity(ResolveResult symbol)
		{
			TypeResolveResult trr = symbol as TypeResolveResult;
			if (trr != null)
				return trr.Type.GetDefinition();
			MemberResolveResult mrr = symbol as MemberResolveResult;
			if (mrr != null)
				return mrr.Member.MemberDefinition;
			return null;
		}
	}
}

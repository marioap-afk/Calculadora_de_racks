using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Cantilever;
using RackCad.Plugin.Drawing;

namespace I52Auth15.HostHarness
{
    /// <summary>What AUTH-15 returned, copied out of its internal result struct by reflection.</summary>
    internal sealed class CreationResult
    {
        public bool IsSuccess { get; set; }

        public ObjectId DefinitionId { get; set; }

        public string BlockName { get; set; }

        public string Failure { get; set; }

        public IReadOnlyList<HeaderBlockInstance> MissingInstances { get; set; }

        public string Diagnostic { get; set; }

        /// <summary>The call itself threw. AUTH-15 must return typed failures, so this is always a finding.</summary>
        public string Thrown { get; set; }

        public string Describe() =>
            Thrown != null
                ? "THREW " + Thrown
                : "IsSuccess=" + IsSuccess + " Failure=" + Failure + " BlockName=" + (BlockName ?? "<null>")
                  + " DefinitionId=" + (DefinitionId.IsNull ? "Null" : DefinitionId.Handle.ToString())
                  + " Missing=" + (MissingInstances == null ? 0 : MissingInstances.Count);
    }

    /// <summary>
    /// The reflection seam onto the INTERNAL AUTH-15 surface (<c>RackDefinitionCreator.CreateInTransaction</c> x2 and
    /// <c>RackDefinitionCreationResult</c>). There is no InternalsVisibleTo: the harness sees exactly what the product
    /// would, through the same assembly that is loaded from the versioned run folder.
    /// </summary>
    internal sealed class Auth15Binding
    {
        public const string CreatorTypeName = "RackCad.Plugin.Systems.Shared.RackDefinitionCreator";
        public const string BlockDataTypeName = "RackCad.Plugin.Systems.Shared.RackBlockData";

        private Auth15Binding()
        {
        }

        public Assembly Plugin { get; private set; }

        public Type Creator { get; private set; }

        public MethodInfo HeaderRunOverload { get; private set; }

        public MethodInfo CantileverOverload { get; private set; }

        public int OverloadCount { get; private set; }

        public string DictKey { get; private set; }

        private MethodInfo _read;

        private PropertyInfo _isSuccess;
        private PropertyInfo _definitionId;
        private PropertyInfo _blockName;
        private PropertyInfo _failure;
        private PropertyInfo _missing;
        private PropertyInfo _diagnostic;

        public static Auth15Binding Bind(Assembly plugin, List<string> problems)
        {
            var binding = new Auth15Binding { Plugin = plugin };
            binding.Creator = plugin.GetType(CreatorTypeName, false);

            if (binding.Creator == null)
            {
                problems.Add("type not found: " + CreatorTypeName);
                return binding;
            }

            const BindingFlags all = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            binding.OverloadCount = binding.Creator.GetMethods(all).Count(m => m.Name == "CreateInTransaction");

            binding.HeaderRunOverload = binding.Creator.GetMethod(
                "CreateInTransaction", all, null,
                new[] { typeof(Database), typeof(Transaction), typeof(LateralHeaderDrawer), typeof(HeaderRunPlan), typeof(string), typeof(RackEmbedDocument) },
                null);

            binding.CantileverOverload = binding.Creator.GetMethod(
                "CreateInTransaction", all, null,
                new[] { typeof(Database), typeof(Transaction), typeof(CantileverViewPlan), typeof(string), typeof(RackEmbedDocument) },
                null);

            var resultType = binding.HeaderRunOverload?.ReturnType;

            if (resultType != null)
            {
                const BindingFlags instance = BindingFlags.Instance | BindingFlags.Public;
                binding._isSuccess = resultType.GetProperty("IsSuccess", instance);
                binding._definitionId = resultType.GetProperty("DefinitionId", instance);
                binding._blockName = resultType.GetProperty("BlockName", instance);
                binding._failure = resultType.GetProperty("Failure", instance);
                binding._missing = resultType.GetProperty("MissingInstances", instance);
                binding._diagnostic = resultType.GetProperty("Diagnostic", instance);

                if (new[] { binding._isSuccess, binding._definitionId, binding._blockName, binding._failure, binding._missing, binding._diagnostic }.Any(p => p == null))
                {
                    problems.Add("RackDefinitionCreationResult is missing one of IsSuccess/DefinitionId/BlockName/Failure/MissingInstances/Diagnostic");
                }
            }

            var blockData = plugin.GetType(BlockDataTypeName, false);
            var dictKey = blockData?.GetField("DictKey", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            binding.DictKey = dictKey?.GetRawConstantValue() as string;
            binding._read = blockData?.GetMethod(
                "Read", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null,
                new[] { typeof(Transaction), typeof(ObjectId) }, null);

            if (binding.DictKey == null)
            {
                problems.Add("RackBlockData.DictKey not found");
            }

            return binding;
        }

        public bool Ready => HeaderRunOverload != null && CantileverOverload != null && _isSuccess != null && DictKey != null && _read != null;

        /// <summary>The product's own reader (<c>RackBlockData.Read</c>) for the envelope on a definition.</summary>
        public string ReadBack(Transaction transaction, ObjectId definitionId) =>
            (string)_read.Invoke(null, new object[] { transaction, definitionId });

        public CreationResult HeaderRun(
            Database database, Transaction transaction, LateralHeaderDrawer drawer, HeaderRunPlan plan, string name, RackEmbedDocument envelope)
            => Invoke(HeaderRunOverload, new object[] { database, transaction, drawer, plan, name, envelope });

        public CreationResult Cantilever(
            Database database, Transaction transaction, CantileverViewPlan plan, string name, RackEmbedDocument envelope)
            => Invoke(CantileverOverload, new object[] { database, transaction, plan, name, envelope });

        private CreationResult Invoke(MethodInfo method, object[] arguments)
        {
            try
            {
                var boxed = method.Invoke(null, arguments);
                return new CreationResult
                {
                    IsSuccess = (bool)_isSuccess.GetValue(boxed),
                    DefinitionId = (ObjectId)_definitionId.GetValue(boxed),
                    BlockName = (string)_blockName.GetValue(boxed),
                    Failure = _failure.GetValue(boxed).ToString(),
                    MissingInstances = (IReadOnlyList<HeaderBlockInstance>)_missing.GetValue(boxed),
                    Diagnostic = (string)_diagnostic.GetValue(boxed),
                };
            }
            catch (TargetInvocationException ex)
            {
                var inner = ex.InnerException ?? ex;
                return new CreationResult { Thrown = inner.GetType().FullName + ": " + inner.Message, Failure = "<threw>" };
            }
        }
    }
}

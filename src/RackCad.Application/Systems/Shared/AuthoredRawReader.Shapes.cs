using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Persistence;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Shared;

// Closed raw vocabulary. These checks run before deserialization or any authored exclusion.
internal static partial class AuthoredRawReader
{
    private static void DynamicRackSystemDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "PALLETFRONT": Number(value, p); break;
                case "PALLETDEPTH": Number(value, p); break;
                case "PALLETHEIGHT": Number(value, p); break;
                case "PALLETWEIGHT": Number(value, p); break;
                case "PALLETWEIGHTUNIT": String(value, p); break;
                case "PALLETSDEEP": Integer(value, p); break;
                case "LOADLEVELS": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "FIRSTLEVELHEIGHT": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "FIRSTLEVELDATUM": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "BEAMDEPTH": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "INTERMEDIATEBEAMDEPTHS": Array(value, p, (v, p) => Number(v, p)); break;
                case "PALLETTOLERANCE": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "INOUTBEAMCATALOGID": String(value, p); break;
                case "HEADERPOSTCATALOGID": String(value, p); break;
                case "POSTPERALTE": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "SEPARATORCOUNTOVERRIDE": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "SEPARATORSPACINGOVERRIDE": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "DERIVEDPOSTREINFORCED": Boolean(value, p); break;
                case "DERIVEDPOSTREINFORCEMENTHEIGHT": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "DERIVEDPOSTHEIGHT": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "HEADERLINEOVERRIDES": Array(value, p, (v, p) => DynamicHeaderLineOverrideDocument(v, p)); break;
                case "DERIVEDPOSTLINEOVERRIDES": Array(value, p, (v, p) => DynamicDerivedPostLineOverrideDocument(v, p)); break;
                case "MANUALHEADERHEIGHTOVERRIDE": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "NUMBERFRONTS": Nullable(value, p, (v, p) => Boolean(v, p)); break;
                case "NUMBERLEVELS": Nullable(value, p, (v, p) => Boolean(v, p)); break;
                case "DRAWRACKNAME": Nullable(value, p, (v, p) => Boolean(v, p)); break;
                case "ANNOTATIONSCALE": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "DIMENSIONS": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "DIMENSIONVIEWS": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "DIMENSIONSTYLE": String(value, p); break;
                case "SAFETYSELECTIONS": Array(value, p, (v, p) => SafetySelectionDocument(v, p)); break;
                case "FRONTS": Array(value, p, (v, p) => DynamicRackFrontDocument(v, p)); break;
                case "MODULES": Array(value, p, (v, p) => DynamicRackModuleDocument(v, p)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void DynamicHeaderLineOverrideDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "POSTINDEX": Integer(value, p); break;
                case "MODULEID": String(value, p); break;
                case "HEADER": Nullable(value, p, (v, q) => RackFrameProjectDocument(v, q)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void RackFrameProjectDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "SCHEMAVERSION": Schema(value, p, false); break;
                case "NAME": String(value, p); break;
                case "UNITS": String(value, p); break;
                case "HEIGHT": Number(value, p); break;
                case "DEPTH": Number(value, p); break;
                case "POSTPERALTE": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "CELOSIASTARTTROQUEL": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "DIAGONALSTARTOFFSETTROQUELES": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "DIAGONALENDOFFSETTROQUELES": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "DIAGONALDOUBLESPACINGTROQUELES": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "HORIZONTALDOUBLEOFFSETTROQUELES": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "PASOTROQUEL": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "PANELCLEAR": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "STANDARDBASELINEID": String(value, p); break;
                case "STANDARDBASELINEVERSION": String(value, p); break;
                case "LEFTPOST": Nullable(value, p, (v, q) => PostDocument(v, q)); break;
                case "RIGHTPOST": Nullable(value, p, (v, q) => PostDocument(v, q)); break;
                case "LEFTBASEPLATE": Nullable(value, p, (v, q) => PlateDocument(v, q)); break;
                case "RIGHTBASEPLATE": Nullable(value, p, (v, q) => PlateDocument(v, q)); break;
                case "HORIZONTALS": Array(value, p, (v, p) => HorizontalDocument(v, p)); break;
                case "PANELS": Array(value, p, (v, p) => PanelDocument(v, p)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void PostDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "POSTCATALOGID": String(value, p); break;
                case "DESCRIPTION": String(value, p); break;
                case "HASREINFORCEMENT": Boolean(value, p); break;
                case "REINFORCEMENTCATALOGID": String(value, p); break;
                case "REINFORCEMENTHEIGHT": Number(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void PlateDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "PLATECATALOGID": String(value, p); break;
                case "DESCRIPTION": String(value, p); break;
                case "CONNECTIONPOINTID": String(value, p); break;
                case "PERALTEOVERRIDE": Nullable(value, p, (v, p) => Number(v, p)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void HorizontalDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "ID": String(value, p); break;
                case "NUMBER": Integer(value, p); break;
                case "ELEVATION": Number(value, p); break;
                case "PROFILEID": String(value, p); break;
                case "QUANTITY": Integer(value, p); break;
                case "MOUNTINGFACE": EnumValue<RackCad.Domain.RackFrames.FrameSide>(value, p); break;
                case "STATE": EnumValue<RackCad.Domain.RackFrames.FrameComponentState>(value, p); break;
                case "NOTES": String(value, p); break;
                case "ISSTANDARD": Boolean(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void PanelDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "PANELID": String(value, p); break;
                case "NUMBER": Integer(value, p); break;
                case "LOWERHORIZONTALID": String(value, p); break;
                case "UPPERHORIZONTALID": String(value, p); break;
                case "ARRANGEMENT": EnumValue<RackCad.Domain.RackFrames.BracingPattern>(value, p); break;
                case "MOUNTINGFACE": EnumValue<RackCad.Domain.RackFrames.FrameSide>(value, p); break;
                case "DIAGONALPROFILEID": String(value, p); break;
                case "DIAGONALDIRECTION": EnumValue<RackCad.Domain.RackFrames.DiagonalDirection>(value, p); break;
                case "STARTCONNECTIONPOINTID": String(value, p); break;
                case "ENDCONNECTIONPOINTID": String(value, p); break;
                case "ISSTANDARD": Boolean(value, p); break;
                case "ISEXCEPTION": Boolean(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void DynamicDerivedPostLineOverrideDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "POSTINDEX": Integer(value, p); break;
                case "HEIGHT": Number(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void SafetySelectionDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "ELEMENTID": String(value, p); break;
                case "QUANTITY": Integer(value, p); break;
                case "SIDE": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "POSTSIDES": Array(value, p, (v, p) => PostSideDocument(v, p)); break;
                case "TOPESHARED": Nullable(value, p, (v, p) => Boolean(v, p)); break;
                case "TOPESAQUE": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "TOPEFRONTAL": Nullable(value, p, (v, p) => Boolean(v, p)); break;
                case "TOPEFONDO": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "TOPEOFFCELLS": Array(value, p, (v, p) => GridCellDocument(v, p)); break;
                case "DESVIADORLONGITUD": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "DESVIADORPRIMERNIVELALTURA": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "DESVIADOROFFCELLS": Array(value, p, (v, p) => GridCellDocument(v, p)); break;
                case "DEFENSAPOSTS": Array(value, p, (v, p) => PostDefenseDocument(v, p)); break;
                case "GUIAENTRADAOFFCELLS": Array(value, p, (v, p) => GridCellDocument(v, p)); break;
                case "PARRILLAFRONTAL": Nullable(value, p, (v, p) => Boolean(v, p)); break;
                case "PARRILLALATERAL": Nullable(value, p, (v, p) => Boolean(v, p)); break;
                case "PARRILLAFRENTE": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "PARRILLACANTIDAD": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "PARRILLAOFFCELLS": Array(value, p, (v, p) => GridCellDocument(v, p)); break;
                case "BOTAPLACEMENT": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "BOTAPOSTS": Array(value, p, (v, p) => BootPostDocument(v, p)); break;
                case "BOTABPLACEMENT": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "BOTABPOSTS": Array(value, p, (v, p) => BootPostDocument(v, p)); break;
                case "BOTASIDESDECLARED": Nullable(value, p, (v, p) => Boolean(v, p)); break;
                case "BOTAPIECEID": String(value, p); break;
                case "BOTABPIECEID": String(value, p); break;
                case "AUTHOREDSIDE": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "DERIVEDAISLES": Array(value, p, (v, p) => PostSideDocument(v, p)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void PostSideDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "POSTINDEX": Integer(value, p); break;
                case "SIDE": Nullable(value, p, (v, p) => Integer(v, p)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void GridCellDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "FRENTE": Integer(value, p); break;
                case "LEVEL": Integer(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void PostDefenseDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "POSTINDEX": Integer(value, p); break;
                case "EXITLENGTH": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "ENTRANCELENGTH": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "EXITAUTO": Nullable(value, p, (v, p) => Boolean(v, p)); break;
                case "ENTRANCEAUTO": Nullable(value, p, (v, p) => Boolean(v, p)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void BootPostDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "POSTINDEX": Integer(value, p); break;
                case "PLACEMENT": Integer(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void DynamicRackFrontDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "ISACTIVE": Nullable(value, p, (v, p) => Boolean(v, p)); break;
                case "PALLETCOUNT": Integer(value, p); break;
                case "LOADLEVELS": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "PALLETSDEEP": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "DEPTHSTARTPOSITION": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "BEAMLENGTHOVERRIDE": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "FIRSTLEVELHEIGHT": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "BFR": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "INTERMEDIATEBEAMDEPTHS": Array(value, p, (v, p) => Number(v, p)); break;
                case "LEVELS": Array(value, p, (v, p) => DynamicRackLevelDocument(v, p)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void DynamicRackLevelDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "PALLETFRONT": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "PALLETHEIGHT": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "PALLETWEIGHT": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "CLEARHEIGHT": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "INOUTBEAMCATALOGID": String(value, p); break;
                case "INOUTBEAMDEPTH": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "BEAMLENGTHOVERRIDE": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "INTERMEDIATEBEAMCATALOGID": String(value, p); break;
                case "INTERMEDIATEBEAMDEPTH": Nullable(value, p, (v, p) => Number(v, p)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void DynamicRackModuleDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "MODULEID": String(value, p); break;
                case "KIND": EnumValue<RackCad.Domain.Systems.Dynamic.DynamicRackModuleKind>(value, p); break;
                case "LENGTH": Number(value, p); break;
                case "ISCALCULATED": Boolean(value, p); break;
                case "ISMANUALOVERRIDE": Boolean(value, p); break;
                case "USECALCULATEDHEADERCONFIGURATION": Nullable(value, p, (v, p) => Boolean(v, p)); break;
                case "NOTES": String(value, p); break;
                case "HEADER": Nullable(value, p, (v, q) => RackFrameProjectDocument(v, q)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void PushBackDesignDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "SCHEMAVERSION": Schema(value, p, false); break;
                case "STRUCTURE": Nullable(value, p, (v, q) => DynamicRackSystemDocument(v, q)); break;
                case "FRONTS": Array(value, p, (v, p) => PushBackFrontDocument(v, p)); break;
                case "LEGACYHIGHENDBEAMPERALTE": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "REARTOPESAQUE": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "REARTOPEPIECEID": String(value, p); break;
                case "DEFENSEPIECEID": String(value, p); break;
                case "REARTOPEOFFCELLS": Array(value, p, (v, p) => PushBackCellDocument(v, p)); break;
                case "SIDEB": Nullable(value, p, (v, q) => PushBackSideDocument(v, q)); break;
                case "COMPOSITE": Nullable(value, p, (v, q) => PushBackCompositeDocument(v, q)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void PushBackFrontDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "HIGHENDBEAMPERALTES": Array(value, p, (v, p) => Nullable(v, p, (v, p) => Number(v, p))); break;
                case "DEFAULTPALLETSDEEP": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "PALLETSDEEPOVERRIDES": Array(value, p, (v, p) => Nullable(v, p, (v, p) => Integer(v, p))); break;
                case "DRAWPALLETS": Array(value, p, (v, p) => Nullable(v, p, (v, p) => Boolean(v, p))); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void PushBackCellDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "FRENTE": Integer(value, p); break;
                case "LEVEL": Integer(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void PushBackSideDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "ISPRESENT": Nullable(value, p, (v, p) => Boolean(v, p)); break;
                case "LOADLEVELS": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "FIRSTLEVELHEIGHT": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "LEGACYHIGHENDBEAMPERALTE": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "FRONTS": Array(value, p, (v, p) => Nullable(v, p, (w, q) => DynamicRackFrontDocument(w, q))); break;
                case "FRONTCONFIGS": Array(value, p, (v, p) => Nullable(v, p, (w, q) => PushBackFrontDocument(w, q))); break;
                case "REARTOPESAQUE": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "REARTOPEPIECEID": String(value, p); break;
                case "DEFENSEPIECEID": String(value, p); break;
                case "REARTOPEOFFCELLS": Array(value, p, (v, p) => PushBackCellDocument(v, p)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void PushBackCompositeDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "GAP": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "CENTRALSEPARATOR": Nullable(value, p, (v, p) => Boolean(v, p)); break;
                case "STRUCTUREOVERRIDEA": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "STRUCTUREOVERRIDEB": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "DEFAULTTOPOLOGY": String(value, p); break;
                case "DEFAULTDIRECTION": String(value, p); break;
                case "TOPOLOGIES": Array(value, p, (v, p) => PushBackTopologyCellDocument(v, p)); break;
                case "ABSENTSLOTSA": Array(value, p, (v, p) => Integer(v, p)); break;
                case "ABSENTSLOTSB": Array(value, p, (v, p) => Integer(v, p)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void PushBackTopologyCellDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "FRENTE": Integer(value, p); break;
                case "LEVEL": Integer(value, p); break;
                case "TOPOLOGY": String(value, p); break;
                case "DIRECTION": String(value, p); break;
                case "CORRIDADEPTH": Nullable(value, p, (v, p) => Integer(v, p)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverLineDocument(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "SCHEMAVERSION": Schema(value, p, false); break;
                case "LINE": Nullable(value, p, (v, q) => CantileverLineDesign(v, q)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverLineDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "ID": Identity(value, p); break;
                case "NAME": String(value, p); break;
                case "STATIONCOUNT": Integer(value, p); break;
                case "COLUMNCENTRESPACING": Number(value, p); break;
                case "STATIONTOPOLOGY": Nullable(value, p, (v, q) => CantileverLineStationTopologyDesign(v, q)); break;
                case "DEFAULTARMTEMPLATE": Nullable(value, p, (v, q) => CantileverArmTemplateDesign(v, q)); break;
                case "ARMCELLOVERRIDES": Array(value, p, (v, p) => CantileverArmCellOverride(v, p)); break;
                case "BRACING": Nullable(value, p, (v, q) => CantileverBracingDesign(v, q)); break;
                case "PLANTAVISIBILITY": Nullable(value, p, (v, q) => CantileverPlantaVisibilityDesign(v, q)); break;
                case "INTERVALCOUNT": Integer(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverLineStationTopologyDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "FACEMODE": EnumValue<RackCad.Domain.Systems.Cantilever.CantileverStationFaceMode>(value, p); break;
                case "SINGLESIDE": EnumValue<RackCad.Domain.Systems.Cantilever.CantileverArmSide>(value, p); break;
                case "COLUMNBASETEMPLATE": Nullable(value, p, (v, q) => CantileverStationColumnBaseTemplateDesign(v, q)); break;
                case "LEVELCOUNT": Integer(value, p); break;
                case "FIRSTLEVELPUNCHINDEX": Integer(value, p); break;
                case "REQUESTEDCLEARHEIGHT": Number(value, p); break;
                case "TOPCLEARFACTOR": Number(value, p); break;
                case "COLUMNHEIGHT": Nullable(value, p, (v, q) => CantileverStationColumnHeightDesign(v, q)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverStationColumnBaseTemplateDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "COLUMNSECTIONID": String(value, p); break;
                case "COLUMNBOTTOMPLATE": Nullable(value, p, (v, q) => CantileverPlateDesign(v, q)); break;
                case "BASE": Nullable(value, p, (v, q) => CantileverBaseDesign(v, q)); break;
                case "CONNECTION": Nullable(value, p, (v, q) => CantileverColumnBaseConnectionDesign(v, q)); break;
                case "BASEFOLLOWSCOLUMN": Boolean(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverPlateDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "THICKNESS": Number(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverBaseDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "SECTIONID": String(value, p); break;
                case "LENGTH": Number(value, p); break;
                case "FRONTPLATE": Nullable(value, p, (v, q) => CantileverPlateDesign(v, q)); break;
                case "REARPLATE": Nullable(value, p, (v, q) => CantileverPlateDesign(v, q)); break;
                case "GUSSET": Nullable(value, p, (v, q) => CantileverGussetDesign(v, q)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverGussetDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "THICKNESS": Number(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverColumnBaseConnectionDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "VARIANT": EnumValue<RackCad.Domain.Systems.Cantilever.CantileverColumnBaseVariantKind>(value, p); break;
                case "PUNCHES": Nullable(value, p, (v, q) => CantileverPunchParameters(v, q)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverPunchParameters(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "DIAMETER": Number(value, p); break;
                case "HORIZONTALENDOFFSET": Number(value, p); break;
                case "CONNECTIONPITCH": Number(value, p); break;
                case "REARPLATEVERTICALENDOFFSET": Number(value, p); break;
                case "REGULARCOLUMNPITCH": Number(value, p); break;
                case "CONNECTIONPUNCHESABOVEBASE": Integer(value, p); break;
                case "COLUMNBOTTOMPLATEPITCH": Number(value, p); break;
                case "COLUMNBOTTOMPLATEENDOFFSET": Nullable(value, p, (v, p) => Number(v, p)); break;
                case "COLUMNTOPPUNCHOFFSET": Nullable(value, p, (v, p) => Number(v, p)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverStationColumnHeightDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "MODE": EnumValue<RackCad.Domain.Systems.Cantilever.CantileverStationColumnHeightMode>(value, p); break;
                case "MANUALHEIGHT": Nullable(value, p, (v, p) => Number(v, p)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverArmTemplateDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "BODY": Nullable(value, p, (v, q) => CantileverArmBodyDesign(v, q)); break;
                case "MOUNTINGPLATE": Nullable(value, p, (v, q) => CantileverArmMountingPlateTemplateDesign(v, q)); break;
                case "ENDPLATE": Nullable(value, p, (v, q) => CantileverArmEndPlateDesign(v, q)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverArmBodyDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "ARRANGEMENT": EnumValue<RackCad.Domain.Systems.Cantilever.CantileverArmBodyArrangement>(value, p); break;
                case "SECTIONID": String(value, p); break;
                case "CUTLENGTH": Number(value, p); break;
                case "SLOPERISEPER12": Number(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverArmMountingPlateTemplateDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "THICKNESS": Number(value, p); break;
                case "VERTICALPUNCHCOUNT": Integer(value, p); break;
                case "VERTICALENDOFFSET": Nullable(value, p, (v, p) => Number(v, p)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverArmEndPlateDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "MODE": EnumValue<RackCad.Domain.Systems.Cantilever.CantileverArmEndPlateMode>(value, p); break;
                case "THICKNESS": Number(value, p); break;
                case "EXTRASTOPHEIGHT": Number(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverArmCellOverride(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "STATIONINDEX": Integer(value, p); break;
                case "LEVELINDEX": Integer(value, p); break;
                case "SIDE": EnumValue<RackCad.Domain.Systems.Cantilever.CantileverArmSide>(value, p); break;
                case "ARM": Nullable(value, p, (v, q) => CantileverArmTemplateDesign(v, q)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverBracingDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "SEPARATORSECTIONID": String(value, p); break;
                case "PANELCOUNTMODE": EnumValue<RackCad.Domain.Systems.Cantilever.CantileverBracedPanelCountMode>(value, p); break;
                case "MANUALPANELCOUNT": Nullable(value, p, (v, p) => Integer(v, p)); break;
                case "BRACEDPANELHEIGHT": Number(value, p); break;
                case "CENTRALEMPTYSPACEHEIGHT": Number(value, p); break;
                case "BRACEKIND": EnumValue<RackCad.Domain.Systems.Cantilever.CantileverBraceBodyKind>(value, p); break;
                case "BRACESECTIONID": String(value, p); break;
                case "COLDROLLED": Nullable(value, p, (v, q) => CantileverColdRolledBraceDesign(v, q)); break;
                case "PANELLAYOUTMODE": EnumValue<RackCad.Domain.Systems.Cantilever.CantileverPanelLayoutMode>(value, p); break;
                case "ADVANCEDPANELSEGMENTS": Array(value, p, (v, p) => CantileverPanelSegmentDesign(v, p)); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverColdRolledBraceDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "DIAMETER": Number(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverPanelSegmentDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "STARTELEVATION": Number(value, p); break;
                case "ENDELEVATION": Number(value, p); break;
                case "BRACINGMODE": EnumValue<RackCad.Domain.Systems.Cantilever.CantileverPanelBracingMode>(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

    private static void CantileverPlantaVisibilityDesign(JsonElement node, string path)
    {
        foreach (var member in Members(node, path))
        {
            string p = path + "." + member.Name;
            var value = member.Value;
            switch (member.Name.ToUpperInvariant())
            {
                case "SHOWARMS": Boolean(value, p); break;
                case "SHOWBRACES": Boolean(value, p); break;
                default: throw Invalid(p, "unknown member");
            }
        }
    }

}

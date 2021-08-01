﻿namespace XNAShaderDecompiler
{
    public enum RenderStateType
    {
        /* Note that we are NOT using the actual RS values from D3D here.
         * For some reason, in the binary data, it's 0-based.
         * Even worse, it doesn't even seem to be in order.
         * Here is the list of changes compared to the real D3DRS enum:
         * - All of the RS_WRAP values are in a row, not separate!
         *
         * -flibit
         */
        RS_ZENABLE,
        RS_FILLMODE,
        RS_SHADEMODE,
        RS_ZWRITEENABLE,
        RS_ALPHATESTENABLE,
        RS_LASTPIXEL,
        RS_SRCBLEND,
        RS_DESTBLEND,
        RS_CULLMODE,
        RS_ZFUNC,
        RS_ALPHAREF,
        RS_ALPHAFUNC,
        RS_DITHERENABLE,
        RS_ALPHABLENDENABLE,
        RS_FOGENABLE,
        RS_SPECULARENABLE,
        RS_FOGCOLOR,
        RS_FOGTABLEMODE,
        RS_FOGSTART,
        RS_FOGEND,
        RS_FOGDENSITY,
        RS_RANGEFOGENABLE,
        RS_STENCILENABLE,
        RS_STENCILFAIL,
        RS_STENCILZFAIL,
        RS_STENCILPASS,
        RS_STENCILFUNC,
        RS_STENCILREF,
        RS_STENCILMASK,
        RS_STENCILWRITEMASK,
        RS_TEXTUREFACTOR,
        RS_WRAP0,
        RS_WRAP1,
        RS_WRAP2,
        RS_WRAP3,
        RS_WRAP4,
        RS_WRAP5,
        RS_WRAP6,
        RS_WRAP7,
        RS_WRAP8,
        RS_WRAP9,
        RS_WRAP10,
        RS_WRAP11,
        RS_WRAP12,
        RS_WRAP13,
        RS_WRAP14,
        RS_WRAP15,
        RS_CLIPPING,
        RS_LIGHTING,
        RS_AMBIENT,
        RS_FOGVERTEXMODE,
        RS_COLORVERTEX,
        RS_LOCALVIEWER,
        RS_NORMALIZENORMALS,
        RS_DIFFUSEMATERIALSOURCE,
        RS_SPECULARMATERIALSOURCE,
        RS_AMBIENTMATERIALSOURCE,
        RS_EMISSIVEMATERIALSOURCE,
        RS_VERTEXBLEND,
        RS_CLIPPLANEENABLE,
        RS_POINTSIZE,
        RS_POINTSIZE_MIN,
        RS_POINTSPRITEENABLE,
        RS_POINTSCALEENABLE,
        RS_POINTSCALE_A,
        RS_POINTSCALE_B,
        RS_POINTSCALE_C,
        RS_MULTISAMPLEANTIALIAS,
        RS_MULTISAMPLEMASK,
        RS_PATCHEDGESTYLE,
        RS_DEBUGMONITORTOKEN,
        RS_POINTSIZE_MAX,
        RS_INDEXEDVERTEXBLENDENABLE,
        RS_COLORWRITEENABLE,
        RS_TWEENFACTOR,
        RS_BLENDOP,
        RS_POSITIONDEGREE,
        RS_NORMALDEGREE,
        RS_SCISSORTESTENABLE,
        RS_SLOPESCALEDEPTHBIAS,
        RS_ANTIALIASEDLINEENABLE,
        RS_MINTESSELLATIONLEVEL,
        RS_MAXTESSELLATIONLEVEL,
        RS_ADAPTIVETESS_X,
        RS_ADAPTIVETESS_Y,
        RS_ADAPTIVETESS_Z,
        RS_ADAPTIVETESS_W,
        RS_ENABLEADAPTIVETESSELLATION,
        RS_TWOSIDEDSTENCILMODE,
        RS_CCW_STENCILFAIL,
        RS_CCW_STENCILZFAIL,
        RS_CCW_STENCILPASS,
        RS_CCW_STENCILFUNC,
        RS_COLORWRITEENABLE1,
        RS_COLORWRITEENABLE2,
        RS_COLORWRITEENABLE3,
        RS_BLENDFACTOR,
        RS_SRGBWRITEENABLE,
        RS_DEPTHBIAS,
        RS_SEPARATEALPHABLENDENABLE,
        RS_SRCBLENDALPHA,
        RS_DESTBLENDALPHA,
        RS_BLENDOPALPHA,

        /* These aren't really "states", but these numbers are
         * referred to by effectStateType as such.
         */
        RS_VERTEXSHADER = 146,
        RS_PIXELSHADER = 147
    }
}
/*
 *  Copyright (C) 2010-2026 JPEXS, All rights reserved.
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3.0 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library.
 */
package com.jpexs.decompiler.flash.exporters.settings;

import com.jpexs.decompiler.flash.exporters.modes.ImageExportMode;
import com.jpexs.decompiler.flash.exporters.modes.ScriptExportMode;
import com.jpexs.decompiler.flash.exporters.modes.SoundExportMode;

/**
 *
 * @author JPEXS
 */
public class XmlSwfExportSettings {
    public ScriptExportMode as12ExportMode;
    public ImageExportMode imageExportMode;
    public SoundExportMode defineSoundExportMode;

    public XmlSwfExportSettings() {
    }   
        
    public XmlSwfExportSettings(ScriptExportMode as12ExportMode, ImageExportMode imageExportMode, SoundExportMode defineSoundExportMode) {
        if (as12ExportMode != null && as12ExportMode != ScriptExportMode.AS) {
            throw new IllegalArgumentException("Unsupported script export mode");
        }
        this.as12ExportMode = as12ExportMode;
        if (
                imageExportMode != null 
                && imageExportMode != ImageExportMode.PNG_GIF_JPEG
                && imageExportMode != ImageExportMode.PNG_GIF_JPEG_ALPHA
        ) {
            throw new IllegalArgumentException("Unsupported image export mode");
        }        
        this.imageExportMode = imageExportMode;
        if (defineSoundExportMode != null && defineSoundExportMode != SoundExportMode.MP3_WAV_FLV) {
            throw new IllegalArgumentException("Unsupported sound export mode");
        }
        this.defineSoundExportMode = defineSoundExportMode;
    }        
}

/*
 *  Copyright (C) 2010-2026 JPEXS
 * 
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 * 
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
package com.jpexs.helpers;

import com.jpexs.decompiler.flash.configuration.SwfSpecificConfiguration;
import com.jpexs.decompiler.flash.configuration.SwfSpecificCustomConfiguration;
import com.jpexs.decompiler.flash.configuration.enums.GridSnapAccuracy;
import com.jpexs.decompiler.flash.configuration.enums.GuidesSnapAccuracy;
import com.jpexs.decompiler.flash.exporters.modes.ExeExportMode;
import com.jpexs.decompiler.flash.importers.TextImportResizeTextBoundsMode;
import java.awt.Color;
import java.io.IOException;
import java.io.InputStream;
import java.io.InvalidClassException;
import java.io.ObjectInputStream;
import java.io.ObjectStreamClass;
import java.util.Arrays;
import java.util.Calendar;
import java.util.List;

/**
 * ObjectInputStream that limits deserialized classes to safe ones.
 * @author JPEXS
 */
public class AllowedObjectInputStream extends ObjectInputStream {
    private static final List<String> ALLOWED_CLASSES = Arrays.asList(
            "java.lang.String",
            "java.lang.Number",
            "java.lang.Short",
            "java.lang.Long",
            "java.lang.Integer",
            "java.lang.Double",
            "java.lang.Float",
            "java.lang.Boolean",
            "java.util.ArrayList",
            "java.util.LinkedList",
            "java.util.HashSet",
            "java.util.LinkedHashSet",
            "java.util.HashMap",
            "java.util.LinkedHashMap",
            SwfSpecificConfiguration.class.getName(),
            SwfSpecificCustomConfiguration.class.getName(),
            ExeExportMode.class.getName(),
            TextImportResizeTextBoundsMode.class.getName(),
            Color.class.getName(),
            GridSnapAccuracy.class.getName(),
            GuidesSnapAccuracy.class.getName(),
            Calendar.class.getName()
    );

    public AllowedObjectInputStream(InputStream in) throws IOException {
        super(in);
    }

    @Override
    protected Class<?> resolveClass(ObjectStreamClass desc)
            throws IOException, ClassNotFoundException {

        String className = desc.getName();

        if (!ALLOWED_CLASSES.contains(className)) {
            throw new InvalidClassException(
                    "Unauthorized deserialization attempt", className);
        }

        return super.resolveClass(desc);
    }
}

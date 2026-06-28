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
package com.jpexs.decompiler.flash.gui;

import com.jpexs.decompiler.flash.configuration.Configuration;
import com.jpexs.decompiler.flash.exporters.modes.ImageExportMode;
import com.jpexs.decompiler.flash.exporters.modes.ScriptExportMode;
import com.jpexs.decompiler.flash.exporters.modes.SoundExportMode;
import java.awt.BorderLayout;
import java.awt.Container;
import java.awt.FlowLayout;
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.Insets;
import java.awt.Window;
import java.awt.event.ActionEvent;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import javax.swing.ComboBoxModel;
import javax.swing.DefaultComboBoxModel;
import javax.swing.JButton;
import javax.swing.JComboBox;
import javax.swing.JLabel;
import javax.swing.JPanel;
import javax.swing.SwingConstants;

/**
 *
 * @author JPEXS
 */
public class XmlExportDialog extends AppDialog {
    private int result = ERROR_OPTION;
        
    private final Map<Class<? extends Enum>, JComboBox<ComboValue>> combos = new HashMap<>();
    
    private final Map<Class<? extends Enum>, String> enumNames = new HashMap<>();
    
    private final List<Object> allDefaults = Arrays.asList(ScriptExportMode.AS, ImageExportMode.PNG_GIF_JPEG_ALPHA, SoundExportMode.MP3_WAV_FLV);
    
    public XmlExportDialog(Window owner) {
        super(owner);
        
        enumNames.put(ScriptExportMode.class, "as12script");
        enumNames.put(ImageExportMode.class, "image");
        enumNames.put(SoundExportMode.class, "definesound");
        setTitle(translate("dialog.title"));
        JPanel buttonsPanel = new JPanel(new FlowLayout());
        JButton okButton = new JButton(translate("button.ok"));
        okButton.addActionListener(this::okButtonActionPerformed);
        JButton cancelButton = new JButton(translate("button.cancel"));
        cancelButton.addActionListener(this::cancelButtonActionPerformed);
        buttonsPanel.add(okButton);
        buttonsPanel.add(cancelButton);                
        
        JPanel centralPanel = new JPanel(new GridBagLayout());
        GridBagConstraints gbc = new GridBagConstraints();
        
        gbc.gridx = 0;
        gbc.gridy = 0;
        gbc.insets = new Insets(1, 2, 1, 2);
        
        JLabel selectLabel = new JLabel(translate("selectExternal"));
        gbc.gridwidth = 4;
        centralPanel.add(selectLabel, gbc);
        
        gbc.gridwidth = 1;
        gbc.gridy++;
        
        DefaultComboBoxModel<ComboValue> as12ScriptsModel = new DefaultComboBoxModel<>();
        as12ScriptsModel.addElement(new ComboValue(null, translate("xml")));
        as12ScriptsModel.addElement(new ComboValue(ScriptExportMode.AS, translateExport("scripts.as")));
        
        addComboRow(centralPanel, gbc, translate("as12scripts"), "as", as12ScriptsModel, ScriptExportMode.class);
        
        DefaultComboBoxModel<ComboValue> imageModel = new DefaultComboBoxModel<>();
        imageModel.addElement(new ComboValue(null, translate("xml")));
        imageModel.addElement(new ComboValue(ImageExportMode.PNG_GIF_JPEG, translateExport("images.png_gif_jpeg")));
        imageModel.addElement(new ComboValue(ImageExportMode.PNG_GIF_JPEG_ALPHA, translateExport("images.png_gif_jpeg_alpha")));
        
        addComboRow(centralPanel, gbc, translateExport("images"), "image", imageModel, ImageExportMode.class);
        
        DefaultComboBoxModel<ComboValue> soundModel = new DefaultComboBoxModel<>();
        soundModel.addElement(new ComboValue(null, translate("xml")));
        soundModel.addElement(new ComboValue(SoundExportMode.MP3_WAV_FLV, translateExport("sounds.mp3_wav_flv")));
        
        addComboRow(centralPanel, gbc, translate("defineSound"), "sound", soundModel, SoundExportMode.class);
        
        
        
        String config = Configuration.lastSelectedXmlExportFormats.get();
        if (!config.isEmpty()) {
            String[] parts  = config.split(",", -1);
            for (String part : parts) {
                String[] parts2 = part.split("\\.", -1);
                if (parts2.length != 2) {
                    continue;
                }
                String name = parts2[0];
                String value = parts2[1];
                for (Class cls : combos.keySet()) {
                    if (enumNames.get(cls).equals(name)) {
                        JComboBox<ComboValue> combo = combos.get(cls);
                        DefaultComboBoxModel<ComboValue> model = (DefaultComboBoxModel<ComboValue>) combo.getModel();
                        for (int i = 0; i < model.getSize(); i++) {
                            ComboValue cv = model.getElementAt(i);
                            if (cv.value == null && value.equals("none")) {
                                combo.setSelectedIndex(0);
                                break;
                            }
                            if (cv.value != null && cv.value.toString().toLowerCase(Locale.ENGLISH).equals(value)) {
                                combo.setSelectedIndex(i);
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        }
        
        JButton allButton = new JButton(translate("all"));
        allButton.addActionListener(this::allButtonActionPerformed);
        
        JButton noneButton = new JButton(translate("none"));
        noneButton.addActionListener(this::noneButtonActionPerformed);
        
        
        gbc.gridx = 2;
        gbc.insets = new Insets(5, 2, 1, 2);
        gbc.anchor = GridBagConstraints.CENTER;        
        centralPanel.add(noneButton, gbc);
        gbc.gridx++;
        centralPanel.add(allButton, gbc);
        
        Container cnt = getContentPane();
        cnt.setLayout(new BorderLayout());
        cnt.add(centralPanel, BorderLayout.CENTER);
        cnt.add(buttonsPanel, BorderLayout.SOUTH);
        
        pack();
        View.centerScreen(this);
        View.setWindowIcon(this, "exportxml");
        getRootPane().setDefaultButton(okButton);
        setModal(true);
    }        
    
    private void addComboRow(JPanel centralPanel, GridBagConstraints gbc, String label, String icon, ComboBoxModel<ComboValue> model, Class<? extends Enum> enumClass) {
        gbc.fill = GridBagConstraints.NONE;
        
        JLabel scriptsLabel = new JLabel(label);        
        scriptsLabel.setIcon(View.getIcon(icon + "16"));
        scriptsLabel.setHorizontalTextPosition(SwingConstants.LEFT);
        gbc.anchor = GridBagConstraints.LINE_END;
        centralPanel.add(scriptsLabel, gbc);
                
        gbc.gridx++;        
        JLabel arrowLabel = new JLabel(translateExport("arrow"));
        gbc.insets = new Insets(1, 5, 1, 5);
        gbc.anchor = GridBagConstraints.CENTER;
        centralPanel.add(arrowLabel, gbc);
        
        gbc.insets = new Insets(1, 2, 1, 2);
        
        gbc.gridx++;        
        gbc.gridwidth = 2;
        JComboBox<ComboValue> combo = new JComboBox<>(model);
        gbc.anchor = GridBagConstraints.LINE_START;
        gbc.fill = GridBagConstraints.BOTH;
        
        combos.put(enumClass, combo);
        
        centralPanel.add(combo, gbc);
        gbc.gridy++;      
        gbc.gridwidth = 1;
        gbc.gridx = 0;
        gbc.fill = GridBagConstraints.NONE;
    }
    
    
    public <T extends Enum<T>> T getEnumValue(Class<T> cls) {
        if (!combos.containsKey(cls)) {
            return null;
        }
        ComboValue cv = (ComboValue) combos.get(cls).getSelectedItem();
        if (cv == null) {
            return null;
        }
        @SuppressWarnings("unchecked")
        T ret = (T) cv.value;
        return ret;
    }
        
    private void okButtonActionPerformed(ActionEvent evt) {        
        
        List<String> vals = new ArrayList<>();
        for (Class cls : combos.keySet()) {
            String name = enumNames.get(cls);
            ComboValue cv = (ComboValue) combos.get(cls).getSelectedItem();
            if (cv == null || cv.value == null) {
                vals.add(name + "." + "none");
            } else {
                vals.add(name + "." + cv.value.toString().toLowerCase());
            }
        }
        Configuration.lastSelectedXmlExportFormats.set(String.join(",", vals));
        
        result = OK_OPTION;
        setVisible(false);
    }
    
    private void noneButtonActionPerformed(ActionEvent evt) {
        for (Class cls : combos.keySet()) {
            JComboBox<ComboValue> combo = combos.get(cls);
            combo.setSelectedIndex(0);
        }
    }

    private void allButtonActionPerformed(ActionEvent evt) {
        for (Class cls : combos.keySet()) {
            JComboBox<ComboValue> combo = combos.get(cls);
            DefaultComboBoxModel<ComboValue> model = (DefaultComboBoxModel<ComboValue>) combo.getModel();
            for (int i = 0; i < model.getSize(); i++) {
                ComboValue value = model.getElementAt(i);
                if (value.value != null && allDefaults.contains(value.value)) {
                    combo.setSelectedIndex(i);
                    break;
                }
            }
        }
    }

    private void cancelButtonActionPerformed(ActionEvent evt) {
        result = CANCEL_OPTION;
        setVisible(false);
    }
    
    public int showExportDialog() {
        setVisible(true);        
        return result;
    }
    
    private String translateExport(String key) {
        return AppDialog.translateForDialog(key, ExportDialog.class);
    }
    
    private class ComboValue {

        public Object value;
        public String text;

        public ComboValue(Object value, String text) {
            this.value = value;
            this.text = text;
        }

        @Override
        public String toString() {
            return text;
        }
    }
}

package org.mcmodule.mchelper.module.render;

import com.mojang.blaze3d.vertex.PoseStack;
import org.lwjgl.glfw.GLFW;
import org.mcmodule.mchelper.event.annotations.EventTarget;
import org.mcmodule.mchelper.event.events.Render2DEvent;
import org.mcmodule.mchelper.module.Category;
import org.mcmodule.mchelper.module.Module;

public class HUD extends Module {
    public HUD() {
        super("HUD", Category.Render, GLFW.GLFW_KEY_H);
        setEnabled(true);
    }

    @EventTarget
    public void onRender(Render2DEvent event) {
        mc.font.drawShadow(new PoseStack(), "Mc Helper", 4, 4, -1);
    }
}

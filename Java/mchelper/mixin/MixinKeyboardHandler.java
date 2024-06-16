package org.mcmodule.mchelper.mixin;

import net.minecraft.client.KeyboardHandler;
import org.lwjgl.glfw.GLFW;
import org.mcmodule.mchelper.McHelper;
import org.mcmodule.mchelper.event.events.KeyPressEvent;
import org.spongepowered.asm.mixin.Mixin;
import org.spongepowered.asm.mixin.injection.At;
import org.spongepowered.asm.mixin.injection.Inject;
import org.spongepowered.asm.mixin.injection.callback.CallbackInfo;

@Mixin(KeyboardHandler.class)
public class MixinKeyboardHandler {
    @Inject(method = "keyPress", at = @At("HEAD"))
    public void onKey(long p_90894_, int key, int p_90896_, int action, int p_90898_, CallbackInfo ci) {
        if (action == GLFW.GLFW_PRESS && key != GLFW.GLFW_KEY_UNKNOWN) {
            McHelper.getInstance().getEventManager().call(new KeyPressEvent(key));
        }
    }
}

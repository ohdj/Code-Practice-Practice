package org.mcmodule.mchelper.mixin;

import net.minecraft.client.Minecraft;
import net.minecraft.client.multiplayer.ClientPacketListener;
import net.minecraft.client.player.AbstractClientPlayer;
import net.minecraft.client.player.LocalPlayer;
import net.minecraft.network.protocol.game.ServerboundMovePlayerPacket;
import net.minecraft.network.protocol.game.ServerboundPlayerCommandPacket;
import net.minecraft.world.phys.Vec3;
import org.mcmodule.mchelper.McHelper;
import org.mcmodule.mchelper.event.events.MotionUpdateEvent;
import org.spongepowered.asm.mixin.Final;
import org.spongepowered.asm.mixin.Mixin;
import org.spongepowered.asm.mixin.Overwrite;
import org.spongepowered.asm.mixin.Shadow;

@Mixin(LocalPlayer.class)
public abstract class MixinLocalPLayer extends AbstractClientPlayer {
    @Shadow private boolean wasSprinting;

    @Shadow @Final public ClientPacketListener connection;

    @Shadow private boolean wasShiftKeyDown;

    @Shadow protected abstract boolean isControlledCamera();

    @Shadow private double xLast;

    @Shadow private double yLast1;

    @Shadow private double zLast;

    @Shadow private float yRotLast;

    @Shadow private float xRotLast;

    @Shadow private int positionReminder;

    @Shadow private boolean lastOnGround;

    @Shadow private boolean autoJumpEnabled;

    @Shadow @Final protected Minecraft minecraft;

    public MixinLocalPLayer() {
        super(null, null);
    }

    /**
     * @author cubk
     * @reason sb
     */
    @Overwrite
    private void sendPosition() {

        MotionUpdateEvent event = new MotionUpdateEvent(this.getX(), this.getY(), this.getZ(), this.getYRot(), this.getXRot(), this.onGround);
        McHelper.getInstance().getEventManager().call(event);

        boolean flag = this.isSprinting();
        if (flag != this.wasSprinting) {
            ServerboundPlayerCommandPacket.Action serverboundplayercommandpacket$action = flag ? ServerboundPlayerCommandPacket.Action.START_SPRINTING : ServerboundPlayerCommandPacket.Action.STOP_SPRINTING;
            this.connection.send(new ServerboundPlayerCommandPacket(this, serverboundplayercommandpacket$action));
            this.wasSprinting = flag;
        }

        boolean flag3 = this.isShiftKeyDown();
        if (flag3 != this.wasShiftKeyDown) {
            ServerboundPlayerCommandPacket.Action serverboundplayercommandpacket$action1 = flag3 ? ServerboundPlayerCommandPacket.Action.PRESS_SHIFT_KEY : ServerboundPlayerCommandPacket.Action.RELEASE_SHIFT_KEY;
            this.connection.send(new ServerboundPlayerCommandPacket(this, serverboundplayercommandpacket$action1));
            this.wasShiftKeyDown = flag3;
        }

        if (this.isControlledCamera()) {
            double d4 = event.getX() - this.xLast;
            double d0 = event.getY() - this.yLast1;
            double d1 = event.getZ() - this.zLast;
            double d2 = (double)(this.getYRot() - this.yRotLast);
            double d3 = (double)(this.getXRot() - this.xRotLast);
            ++this.positionReminder;
            boolean flag1 = d4 * d4 + d0 * d0 + d1 * d1 > 9.0E-4 || this.positionReminder >= 20;
            boolean flag2 = d2 != 0.0 || d3 != 0.0;
            if (this.isPassenger()) {
                Vec3 vec3 = this.getDeltaMovement();
                this.connection.send(new ServerboundMovePlayerPacket.PosRot(vec3.x, -999.0, vec3.z, event.getYaw(), event.getPitch(), event.isOnGround()));
                flag1 = false;
            } else if (flag1 && flag2) {
                this.connection.send(new ServerboundMovePlayerPacket.PosRot(event.getX(), event.getY(), event.getZ(), event.getYaw(), event.getPitch(), event.isOnGround()));
            } else if (flag1) {
                this.connection.send(new ServerboundMovePlayerPacket.Pos(event.getX(), event.getY(), event.getZ(), event.isOnGround()));
            } else if (flag2) {
                this.connection.send(new ServerboundMovePlayerPacket.Rot(event.getYaw(), event.getPitch(), event.isOnGround()));
            } else if (this.lastOnGround != this.onGround) {
                this.connection.send(new ServerboundMovePlayerPacket.StatusOnly(event.isOnGround()));
            }

            if (flag1) {
                this.xLast = event.getX();
                this.yLast1 = event.getY();
                this.zLast = event.getZ();
                this.positionReminder = 0;
            }

            if (flag2) {
                this.yRotLast = event.getYaw();
                this.xRotLast = event.getPitch();
            }

            this.lastOnGround = event.isOnGround();
            this.autoJumpEnabled = this.minecraft.options.autoJump;
        }

    }
}

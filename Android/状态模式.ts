class MicrowaveOven {
    // 创建所有状态对象
    readonly heating = new HeatingState(this);
    readonly doorOpenState = new DoorOpenState(this);
    readonly doorCloseState = new DoorCloseState(this);

    // 微波炉的状态 初始化为 关门状态
    microwaveState: MicrowaveState = this.doorCloseState;

    OpenDoor(): void {
        this.microwaveState.OpenDoor();
    }

    CloseDoor(): void {
        this.microwaveState.CloseDoor();
    }

    Start(): void {
        this.microwaveState.Start();
    }

    Stop(): void {
        this.microwaveState.Stop();
    }
}

abstract class MicrowaveState {
    // 因为要修改微波炉状态
    // 状态对象都需要引用微波炉
    readonly microwave: MicrowaveOven;
    constructor(microwave: MicrowaveOven) {
        this.microwave = microwave;
    }
    abstract OpenDoor(): void;
    abstract CloseDoor(): void;
    abstract Start(): void;
    abstract Stop(): void;
}

class HeatingState extends MicrowaveState {
    OpenDoor() { console.error('加热时，不能开门') }
    CloseDoor() { console.error('加热时，门已关') }
    Start() { console.error('正在加热') }
    Stop() {
        this.microwave.microwaveState = this.microwave.doorCloseState;
        console.log('微波炉已停止工作...')
    }
}

class DoorOpenState extends MicrowaveState {
    OpenDoor() { console.error('门已开') }
    CloseDoor() { this.microwave.microwaveState = this.microwave.doorCloseState; }
    Start() { this.microwave.microwaveState = this.microwave.heating; }
    Stop() { console.log('门已关，微波炉已停止工作...') }
}

class DoorCloseState extends MicrowaveState {
    OpenDoor() { this.microwave.microwaveState = this.microwave.doorOpenState; }
    CloseDoor() { console.log('门已关') }
    Start() { console.error('门已关，不能开始工作') }
    Stop() { console.log('门已关，微波炉已停止工作...') }
}
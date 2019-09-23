public class CharacterCapability {
    Character character;

    string _stateName;
    public string stateName { // Prevent setting statename externally
        get { return _stateName; }
    }

    bool _initDone = false;
    public CharacterCapability(Character character) {
        if (_initDone) return;
        _initDone = true;
        this.character = character;
        Init();
    }

    public virtual void Init() { }
    public virtual void StateInit(string prevStateName) { }
    public virtual void StateDeinit(string nextStateName) { }
    public virtual void Update(float deltaTime) { }
}
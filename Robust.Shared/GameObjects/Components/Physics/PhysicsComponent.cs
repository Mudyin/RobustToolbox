﻿using System;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Log;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Serialization;

namespace Robust.Shared.GameObjects.Components
{
    [Obsolete("Use the ICollidableComponent interface, or use ICollidableComponent.Anchored if you are using this to check if the entity can be moved.")]
    public interface IPhysicsComponent : IComponent
    {
        /// <summary>
        ///     Current mass of the entity in kilograms.
        /// </summary>
        float Mass { get; set; }

        /// <summary>
        ///     Current linear velocity of the entity in meters per second.
        /// </summary>
        Vector2 LinearVelocity { get; set; }

        /// <summary>
        ///     Current angular velocity of the entity in radians per sec.
        /// </summary>
        float AngularVelocity { get; set; }

        /// <summary>
        ///     Current momentum of the entity in kilogram meters per second
        /// </summary>
        Vector2 Momentum { get; set; }

        /// <summary>
        ///     The current status of the object
        /// </summary>
        BodyStatus Status { get; set; }

        /// <summary>
        ///     Represents a virtual controller acting on the physics component.
        /// </summary>
        VirtualController? Controller { get; }

        /// <summary>
        ///     Whether this component is on the ground
        /// </summary>
        bool OnGround { get; }

        /// <summary>
        ///     Whether or not the entity is anchored in place.
        /// </summary>
        bool Anchored { get; set; }

        event Action? AnchoredChanged;

        bool Predict { get; set; }

        void SetController<T>()
            where T : VirtualController, new();

        void RemoveController();
    }

    [Obsolete("Migrate to CollidableComponent")]
    [RegisterComponent]
    [ComponentReference(typeof(IPhysicsComponent))]
    public class PhysicsComponent : Component, IPhysicsComponent
    {
        private ICollidableComponent _collidableComponent = default!;
        private bool _upgradeCollidable;

        private float _mass;
        private Vector2 _linVelocity;
        private float _angVelocity;
        private BodyStatus _status;
        private VirtualController? _controller;
        private bool _anchored;

        /// <inheritdoc />
        public override string Name => "Physics";

        /// <inheritdoc />
        public override uint? NetID => NetIDs.PHYSICS;

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            if (!Owner.EnsureComponent(out CollidableComponent comp))
            {
                Logger.ErrorS("physics", $"Entity {Owner} is missing a {nameof(CollidableComponent)}, adding one for you.");
            }

            _collidableComponent = comp;

            if (_upgradeCollidable)
            {
                _collidableComponent.Mass = _mass;
                _collidableComponent.LinearVelocity = _linVelocity;
                _collidableComponent.AngularVelocity = _angVelocity;
                _collidableComponent.Anchored = _anchored;
                _collidableComponent.Status = _status;
                _collidableComponent.Controller = _controller;
            }
        }

        /// <inheritdoc />
        public override void ExposeData(ObjectSerializer serializer)
        {
            base.ExposeData(serializer);

            if(serializer.Reading) // Prevents the obsolete component from writing
            {
                if(!serializer.ReadDataField("upgraded", false))
                {
                    _upgradeCollidable = true;
                    serializer.DataField<float>(ref _mass, "mass", 1);
                    serializer.DataField(ref _linVelocity, "vel", Vector2.Zero);
                    serializer.DataField(ref _angVelocity, "avel", 0.0f);
                    serializer.DataField(ref _anchored, "Anchored", false);
                    serializer.DataField(ref _status, "Status", BodyStatus.OnGround);
                    serializer.DataField(ref _controller, "Controller", null);
                }
            }
            else
            {
                var upgrade = true;
                serializer.DataField(ref upgrade, "upgraded", true);
            }
        }

        #region IPhysicsComponent Proxy

        public float Mass
        {
            get => _collidableComponent.Mass;
            set => _collidableComponent.Mass = value;
        }

        public Vector2 LinearVelocity
        {
            get => _collidableComponent.LinearVelocity;
            set => _collidableComponent.LinearVelocity = value;
        }

        public float AngularVelocity
        {
            get => _collidableComponent.AngularVelocity;
            set => _collidableComponent.AngularVelocity = value;
        }

        public Vector2 Momentum
        {
            get => _collidableComponent.Momentum;
            set => _collidableComponent.Momentum = value;
        }

        public BodyStatus Status
        {
            get => _collidableComponent.Status;
            set => _collidableComponent.Status = value;
        }

        public VirtualController? Controller => _collidableComponent.Controller;

        public bool OnGround => _collidableComponent.OnGround;

        public bool Anchored
        {
            get => _collidableComponent.Anchored;
            set => _collidableComponent.Anchored = value;
        }

        public event Action? AnchoredChanged
        {
            add => _collidableComponent.AnchoredChanged += value;
            remove => _collidableComponent.AnchoredChanged -= value;
        }

        public bool Predict
        {
            get => _collidableComponent.Predict;
            set => _collidableComponent.Predict = value;
        }

        public void SetController<T>()
            where T : VirtualController, new()
        {
            _collidableComponent.SetController<T>();
        }

        public void RemoveController()
        {
            _collidableComponent.RemoveController();
        }

        #endregion
    }
}

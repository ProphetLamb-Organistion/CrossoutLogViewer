using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CrossoutLogView.GUI.Events
{
    public delegate void OpenModelViewerEventHandler(object sender, OpenModelViewerEventArgs e);
    public sealed class OpenModelViewerEventArgs : EventArgs, IEquatable<OpenModelViewerEventArgs>
    {
        public OpenModelViewerEventArgs(ViewModelBase viewModel, EventArgs innerEvent = null)
        {
            ViewModel = viewModel;
            InnerEvent = innerEvent;
        }

        public ViewModelBase ViewModel { get; }
        public EventArgs InnerEvent { get; }

        public override bool Equals(object obj) => Equals(obj as OpenModelViewerEventArgs);
        public bool Equals([AllowNull] OpenModelViewerEventArgs other) => other != null && EqualityComparer<ViewModelBase>.Default.Equals(ViewModel, other.ViewModel) && EqualityComparer<EventArgs>.Default.Equals(InnerEvent, other.InnerEvent);
        public override int GetHashCode() => HashCode.Combine(ViewModel, InnerEvent);

        public static bool operator ==(OpenModelViewerEventArgs left, OpenModelViewerEventArgs right) => left.Equals(right);
        public static bool operator !=(OpenModelViewerEventArgs left, OpenModelViewerEventArgs right) => !(left == right);
    }
}

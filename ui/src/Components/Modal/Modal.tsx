import React from "react";

import "./Modal.css";

function Modal(props: any) {
  const handleDragEvents = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
  };

  return (
    <div
      className="modal"
      onClick={() => (props.onClose ? props.onClose() : "")}
      onDragStart={handleDragEvents}
      onDragEnd={handleDragEvents}
      onDragEnter={handleDragEvents}
      onDragOver={handleDragEvents}
      onDrop={handleDragEvents}
    >
      <div
        className="modal-content custom-scroll"
        onClick={(event) => event.stopPropagation()}
        onDragStart={handleDragEvents}
        onDragEnd={handleDragEvents}
        onDragEnter={handleDragEvents}
        onDragOver={handleDragEvents}
        onDrop={handleDragEvents}
      >
        {props.children}
      </div>
    </div>
  );
}

export default Modal;

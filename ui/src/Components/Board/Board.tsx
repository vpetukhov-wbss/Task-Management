import React, { useState } from "react";
import { MoreHorizontal } from "react-feather";

import Card from "../Card/Card";
import Dropdown from "../Dropdown/Dropdown";
import CustomInput from "../CustomInput/CustomInput";

import "./Board.css";
import { IBoard } from "../../Interfaces/Kanban";
import { useAppSelector } from "../../store/hooks";
import { usePermissions } from "../../utils/permissions";

interface BoardProps {
  board: IBoard;
  addCard: (boardId: string, title: string) => void;
  removeBoard: (boardId: string) => void;
  updateBoard: (boardId: string, title: string) => void;
  removeCard: (boardId: string, cardId: string) => void;
  onDragEnd: (boardId: string, cardId: string) => void;
  onDragEnter: (boardId: string, cardId: string) => void;
  onBoardDragStart?: (e: React.DragEvent, boardId: string) => void;
  onBoardDragEnter?: (e: React.DragEvent, boardId: string) => void;
  onBoardDragEnd?: (boardId: string) => void;
  isDraggingBoard?: boolean;
  isTargetBoard?: boolean;
}

function Board(props: BoardProps) {
  const {
    board,
    addCard,
    removeBoard,
    updateBoard,
    removeCard,
    onDragEnd,
    onDragEnter,
    onBoardDragStart,
    onBoardDragEnter,
    onBoardDragEnd,
    isDraggingBoard,
    isTargetBoard,
  } = props;
  const [showDropdown, setShowDropdown] = useState(false);
  const [isEditingTitle, setIsEditingTitle] = useState(false);
  const { user } = useAppSelector((state) => state.auth);
  const permissions = usePermissions(user?.role);

  const handleUpdateTitle = (newTitle: string) => {
    if (newTitle.trim() && newTitle !== board.title) {
      updateBoard(board.id, newTitle);
    }
    setIsEditingTitle(false);
  };

  return (
    <div
      className="board"
      onDragEnter={(e) => onBoardDragEnter?.(e, board.id)}
      onDragOver={(e) => e.preventDefault()}
      style={{
        opacity: isTargetBoard && isDraggingBoard ? 0.5 : 1,
        transition: 'opacity 0.2s ease'
      }}
    >
      <div className="board-inner" key={board?.id}>
        <div
          className="board-header"
          draggable={!isEditingTitle}
          onDragStart={(e) => {
            if (isEditingTitle) {
              e.preventDefault();
              return;
            }
            e.stopPropagation();
            onBoardDragStart?.(e, board.id);
          }}
          onDragEnd={(e) => {
            e.stopPropagation();
            onBoardDragEnd?.(board.id);
          }}
          style={{
            cursor: isEditingTitle ? 'default' : (isDraggingBoard ? 'grabbing' : 'grab')
          }}
        >
          {isEditingTitle ? (
            <CustomInput
              defaultValue={board?.title}
              text={board?.title}
              placeholder="Enter Board Title"
              onSubmit={handleUpdateTitle}
              displayClass="board-header-title-edit"
              editClass="board-header-title-edit-input"
            />
          ) : (
            <p className="board-header-title">
              {board?.title}
              <span>{board?.cards?.length || 0}</span>
            </p>
          )}
          {permissions.canManageBoard && (
            <div
              className="board-header-title-more"
              onClick={(e) => {
                e.stopPropagation();
                setShowDropdown(true);
              }}
            >
              <MoreHorizontal />
              {showDropdown && (
                <Dropdown
                  class="board-dropdown"
                  onClose={() => setShowDropdown(false)}
                >
                  <p onClick={() => {
                    setShowDropdown(false);
                    setIsEditingTitle(true);
                  }}>
                    Edit Board
                  </p>
                  <p onClick={() => {
                    setShowDropdown(false);
                    removeBoard(board?.id);
                  }}>
                    Delete Board
                  </p>
                </Dropdown>
              )}
            </div>
          )}
        </div>
        <div
          className="board-cards custom-scroll"
          onDragOver={(e) => e.preventDefault()}
        >
          {board?.cards?.map((item) => (
            <Card
              key={item.id}
              card={item}
              boardId={board.id}
              removeCard={removeCard}
              onDragEnter={onDragEnter}
              onDragEnd={onDragEnd}
            />
          ))}
          {permissions.canCreateTask && (
            <div key="add-card-input">
              <CustomInput
                text="+ Add Card"
                placeholder="Enter Card Title"
                displayClass="board-add-card"
                editClass="board-add-card-edit"
                onSubmit={(value: string) => addCard(board?.id, value)}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default Board;

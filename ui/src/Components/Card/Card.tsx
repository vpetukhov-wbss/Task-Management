import React, { useState } from "react";
import { AlignLeft, MoreHorizontal } from "react-feather";
import { ICard, TaskPriority, TaskStatus } from "../../Interfaces/Kanban";
import Dropdown from "../Dropdown/Dropdown";

import "./Card.css";
import CardInfo from "./CardInfo/CardInfo";
import { useAppSelector } from "../../store/hooks";
import { usePermissions } from "../../utils/permissions";

interface CardProps {
  card: ICard;
  boardId: string;
  removeCard: (boardId: string, cardId: string) => void;
  onDragEnd: (boardId: string, cardId: string) => void;
  onDragEnter: (boardId: string, cardId: string) => void;
}
function Card(props: CardProps) {
  const { card, boardId, removeCard, onDragEnd, onDragEnter } = props;
  const { id, title, description, priority, priorityName, status, statusName } = card;
  const [showDropdown, setShowDropdown] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const { user } = useAppSelector((state) => state.auth);
  const permissions = usePermissions(user?.role);

  const handleCardClick = (e: React.MouseEvent) => {
    if ((e.target as HTMLElement).closest('.card-top-more')) {
      return;
    }
    setShowModal(true);
  };

  const getPriorityColor = (priority: TaskPriority): string => {
    switch (priority) {
      case TaskPriority.High:
        return '#ff4757';
      case TaskPriority.Medium:
        return '#ffa502';
      case TaskPriority.Low:
        return '#1e90ff';
      default:
        return '#95a5a6';
    }
  };

  const getStatusColor = (status: TaskStatus): string => {
    switch (status) {
      case TaskStatus.Incomplete:
        return '#95a5a6'; // Gray
      case TaskStatus.InProgress:
        return '#3498db'; // Blue
      case TaskStatus.OnHold:
        return '#f39c12'; // Orange
      case TaskStatus.Complete:
        return '#2ed573'; // Green
      default:
        return '#95a5a6';
    }
  };

  const getStatusTextColor = (status: TaskStatus): string => {
    return 'white';
  };

  return (
    <>
      {showModal && (
        <CardInfo
          onClose={() => setShowModal(false)}
          card={card}
          boardId={boardId}
        />
      )}
      <div
        className="card"
        key={card.id}
        draggable={!showModal}
        onDragEnd={() => onDragEnd(boardId, id)}
        onDragEnter={() => onDragEnter(boardId, id)}
        onDragOver={(e) => e.preventDefault()}
        onClick={handleCardClick}
      >
        <div className="card-top">
          <div className="card-top-labels">
            <span
              className="card-priority-badge"
              style={{
                backgroundColor: getPriorityColor(priority),
                color: 'white',
                padding: '2px 8px',
                borderRadius: '4px',
                fontSize: '11px',
                fontWeight: 'bold',
              }}
            >
              {priorityName}
            </span>
            <span
              className="card-status-badge"
              style={{
                backgroundColor: getStatusColor(status),
                color: getStatusTextColor(status),
                padding: '2px 8px',
                borderRadius: '4px',
                fontSize: '11px',
                fontWeight: 'bold',
              }}
            >
              {statusName}
            </span>
          </div>
          {permissions.canManageTask && (
            <div
              className="card-top-more"
              onClick={(event) => {
                event.stopPropagation();
                setShowDropdown(true);
              }}
            >
              <MoreHorizontal />
              {showDropdown && (
                <Dropdown
                  class="board-dropdown"
                  onClose={() => setShowDropdown(false)}
                >
                  <p onClick={() => removeCard(boardId, id)}>Delete Card</p>
                </Dropdown>
              )}
            </div>
          )}
        </div>
        <div className="card-title">{title}</div>
        {description && (
          <div className="card-description">
            <AlignLeft size={14} style={{ marginRight: '4px' }} />
            <span>{description.substring(0, 50)}{description.length > 50 ? '...' : ''}</span>
          </div>
        )}
      </div>
    </>
  );
}

export default Card;
